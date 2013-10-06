'Imports System.Runtime.InteropServices 'size of
'Imports System.Security.Cryptography 'RSA'果然是不行。。'copy有错误，可能不是net的rsa问题，再试试。‘就是不行
Imports Org.BouncyCastle.Crypto.Engines
Imports Org.BouncyCastle.Crypto.Macs
Imports Org.BouncyCastle.Crypto.Parameters
Imports Org.BouncyCastle.Crypto.Digests
Imports System.Xml


Public Class Form1
    Public arrRealms() As String

    Private Const CN = 0
    Private Const TW = 1
    Private Const US = 2
    Private Const EU = 3

    Public CurrentRealm As Integer

    Public sRealm() As realm 'pointer 本地数据

    Dim isMouseDown As Boolean '窗体移动
    Dim position As Point '窗体移动

    Dim initThread As New System.Threading.Thread(AddressOf fInitialize)

    Public Sub ChangeRealm(ByVal intRealm As Integer)
        On Error GoTo err

        Dim sameFlag As Boolean
        If CurrentRealm = intRealm Then
            sameFlag = True
        End If
        CurrentRealm = intRealm

        If sRealm(CurrentRealm).isEnrolled = False Then
            Label1.Text = sRealm(CurrentRealm).realmName & " Enroll Failed"
            Timer1.Enabled = False
            Label2.Text = "00000000"
        Else
            Label1.Text = sRealm(CurrentRealm).serial
            Label3.Text = BitConverter.ToString(sRealm(CurrentRealm).secretKey).Replace("-", "") ''secret
            'Me.Text = 0
            ProgressBar1.Maximum = 30
            ProgressBar1.Step = 1
            ProgressBar1.Value = Single.Parse(ProgressBar1.Maximum) * sRealm(CurrentRealm).getPercent '!!!!!!!!!!!!!!!!!!!!!!
            Timer1.Enabled = True
        End If


        If sameFlag <> True Then
            Dim xmldoc As XmlDocument = New XmlDocument()
            xmldoc.Load("realmInfo.xml")

            Dim xmlNode As XmlNode = xmldoc.SelectSingleNode("Config/Interface")
            xmlNode.Item("CurrentRealm").InnerText = CurrentRealm
            xmldoc.Save("realmInfo.xml")
        End If

        Exit Sub
err:
        MsgBox("xml文件可能被破坏,请手动还原,程序将退出.")
        End

    End Sub
    Private Sub fInitialize()

        For i = 0 To 3
            If sRealm(i).isEnrolled = False Then
                Label2.Text = "Enrolling " & arrRealms(i)
                sRealm(i).enroll()
            End If
        Next

    End Sub

    Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick
        '很扯的方法,responce的异步不好写
        If initThread.IsAlive = False Then
            realm.SaveInfo(sRealm, 4)
            ChangeRealm(CurrentRealm)
            Timer2.Enabled = False

        End If
    End Sub
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        CurrentRealm = CN

        ReDim arrRealms(3)
        arrRealms(0) = "CN"
        arrRealms(1) = "TW"
        arrRealms(2) = "US"
        arrRealms(3) = "EU"

        ReDim sRealm(3)
        sRealm(CN) = New realm("CN")
        sRealm(TW) = New realm("TW")
        sRealm(US) = New realm("US")
        sRealm(EU) = New realm("EU")

        If IO.File.Exists("realmInfo.xml") Then
            '存在配置文件
            If realm.LoadInfo(sRealm) = False Then 'inside change currentrealm
                MsgBox("xml文件读取错误,可能已被修改,请手动还原,程序将退出.")
                End
            End If
        End If

        Control.CheckForIllegalCrossThreadCalls = False
        initThread.Start()
        Timer2.Enabled = True
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        'Me.Text = CStr(ProgressBar1.Maximum * sLocalData.getPercent)
        ProgressBar1.PerformStep()
        Label2.Text = sRealm(CurrentRealm).calcKey  '!!!!!!!!!!!!!!!!!!!!!!
        If ProgressBar1.Value = ProgressBar1.Maximum Then
            ProgressBar1.Value = ProgressBar1.Maximum * sRealm(CurrentRealm).getPercent
        End If
    End Sub

    '禁用鼠标滚轮事件
    'Private Sub ComboBox1_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ComboBox1.MouseWheel
    'Dim wm As HandledMouseEventArgs = e
    '    wm.Handled = True
    'End Sub

    '-----------------------------------------------------------
    'form 的鼠标响应 开始
    '-----------------------------------------------------------
    Private Sub Form1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        If e.Button = MouseButtons.Left Then
            Dim x, y As Integer
            If sender.GetHashCode = Me.Label2.GetHashCode Then
                x = -e.X - Label2.Left
                y = -e.Y - Label2.Top
            ElseIf sender.GetHashCode = Me.Label1.GetHashCode Then
                x = -e.X - Label1.Left
                y = -e.Y - Label1.Top
            ElseIf sender.GetHashCode = Me.ProgressBar1.GetHashCode Then
                x = -e.X - ProgressBar1.Left
                y = -e.Y - ProgressBar1.Top
            Else
                x = -e.X
                y = -e.Y
            End If

            position = New Point(x, y)
            isMouseDown = True
        End If
    End Sub

    Private Sub Form1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        If isMouseDown = True Then

            Dim newPosition As Point = Control.MousePosition
            newPosition.Offset(position)
            Me.Location = newPosition
        End If
    End Sub

    Private Sub Form1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        If e.Button = MouseButtons.Left Then

            isMouseDown = False
        End If
    End Sub
    '-----------------------------------------------------------
    'form 的鼠标响应 结束
    '-----------------------------------------------------------

    '-----------------------------------------------------------
    '其他部件 的鼠标响应 开始
    '-----------------------------------------------------------
    Private Sub Label1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Label1.MouseDown
        Form1_MouseDown(sender, e)
    End Sub

    Private Sub Label1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Label1.MouseMove
        Form1_MouseMove(sender, e)
    End Sub

    Private Sub Label1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Label1.MouseUp
        Form1_MouseUp(sender, e)
    End Sub
    Private Sub Label2_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Label2.MouseDown
        Form1_MouseDown(sender, e)
    End Sub

    Private Sub Label2_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Label2.MouseMove
        Form1_MouseMove(sender, e)
    End Sub

    Private Sub Label2_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Label2.MouseUp
        Form1_MouseUp(sender, e)
    End Sub
    Private Sub ProgressBar1_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ProgressBar1.MouseDown
        Form1_MouseDown(sender, e)
    End Sub

    Private Sub ProgressBar1_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ProgressBar1.MouseMove
        Form1_MouseMove(sender, e)
    End Sub

    Private Sub ProgressBar1_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ProgressBar1.MouseUp
        Form1_MouseUp(sender, e)
    End Sub

    '-----------------------------------------------------------
    '其他部件 的鼠标响应 结束
    '-----------------------------------------------------------


    Private Sub Panel1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Panel1.Click
        End
    End Sub

    Private Sub Panel2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Panel2.Click
        Form2.Show()
    End Sub

    Private Sub Label2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label2.Click
        Clipboard.SetText(Label2.Text)
        'Label2.Text = "已复制到剪贴板"
    End Sub

 
End Class








Public Class realm
    'Inherits dataFunction

    Public realmName As String
    Public isEnrolled As Boolean
    Public serial As String
    Public ServerTimeDiff As Long
    Public secretKey As Byte()
    Private Structure rawData 'enroll提交的数据格式
        Dim rHead As Byte()
        Dim rRand As Byte()
        Dim rLocation As Byte()
        Dim rDevice As Byte()
    End Structure


    'Public locKey As Byte()
    Public Shared Function LoadInfo(ByRef realm() As realm) As Boolean
        '给出realm name 和数量 再调用这个 比较规范,form1.arrRealms 不规范
        On Error GoTo err
        'Dim rXml As XmlTextReader = New XmlTextReader("realmInfo.xml")
        Dim xmldoc As XmlDocument = New XmlDocument()
        xmldoc.Load("realmInfo.xml")

        Dim xmlNodeI As XmlNode = xmldoc.SelectSingleNode("Config/Interface")
        Form1.CurrentRealm = CInt(xmlNodeI.Item("CurrentRealm").InnerText()) '--------------

        For i = 0 To 3 'magic number
            Dim xmlNode As XmlNode = xmldoc.SelectSingleNode("Config/realms/" & Form1.arrRealms(i))
            If xmlNode.Item("isEnrolled").InnerText() = "True" Then
                realm(i).isEnrolled = True
                realm(i).secretKey = hexStr2byteArr(xmlNode.Item("secretKey").InnerText())

                realm(i).serial = xmlNode.Item("Serial").InnerText()
                realm(i).ServerTimeDiff = xmlNode.Item("ServerTimeDiff").InnerText()
            Else
                realm(i).isEnrolled = False
            End If
        Next
        Return True
err:
        Return False

    End Function
    Public Shared Function SaveInfo(ByVal realm() As realm, ByVal count As Integer) As Boolean
        Dim wXml As XmlTextWriter = New XmlTextWriter("realmInfo.xml", System.Text.Encoding.GetEncoding("GB2312"))
        'Dim rXml As XmlTextReader = New XmlTextReader("realmInfo.xml")
        'onerr 一般这里不会出错,就不写了--------------------------
        'rXml.r()
        wXml.Formatting = Xml.Formatting.Indented
        wXml.WriteStartDocument()

        wXml.WriteComment("修改此文档可能导致程序无法运行,或数据丢失.")
        wXml.WriteStartElement("Config")

        wXml.WriteStartElement("Interface")
        wXml.WriteElementString("CurrentRealm", Form1.CurrentRealm)
        wXml.WriteEndElement()

        wXml.WriteStartElement("realms")
        For i = 0 To count - 1
            wXml.WriteStartElement(realm(i).realmName)
            'wXml.WriteAttributeString("serial", "123456")
            If realm(i).isEnrolled = True Then
                wXml.WriteElementString("isEnrolled", realm(i).isEnrolled)
                wXml.WriteElementString("Serial", realm(i).serial)
                wXml.WriteElementString("secretKey", BitConverter.ToString(realm(i).secretKey).Replace("-", ""))
                wXml.WriteElementString("ServerTimeDiff", realm(i).ServerTimeDiff)
            Else
                wXml.WriteElementString("isEnrolled", realm(i).isEnrolled)
            End If
           
            wXml.WriteEndElement()
        Next
        wXml.WriteEndElement()

        wXml.WriteEndElement() 'config
        wXml.Close()
        Return True
    End Function

    Public Sub New(ByVal locationInit As String)
        realmName = locationInit
        isEnrolled = False
    End Sub


    'Protected Structure iStruct
    'Dim realmName As String
    'Dim isEnrolled As Boolean
    'Dim serial As String
    ' 'Dim ServerTimeDiff As Long
    'Dim secretKey As Byte()
    'End Structure

    Public Sub enroll()
        On Error GoTo err
        Dim postData As rawData 'enroll 提交的数据，new的时候有构建函数
        Dim requestURL As String 'enroll url constructor used
        '25hex6-128byte
        Dim strPubKey As String = "955e4bd989f3917d2f15544a7e0504eb9d7bb66b6f8a2fe470e453c779200e5e" & _
                                    "3ad2e43a02d06c4adbd8d328f1a426b83658e88bfd949b2af4eaf30054673a14" & _
                                    "19a250fa4cc1278d12855b5b25818d162c6e6ee2ab4a350d401d78f6ddb99711" & _
                                    "e72626b48bd8b5b0b7f3acf9ea3c9e0005fee59e19136cdb7c83f2ab8b0a2a99"

        ReDim postData.rHead(1)
        postData.rHead(0) = 1

        If realmName = "CN" Then
            requestURL = "http://mobile-service.battlenet.com.cn/enrollment/enroll.htm"
        ElseIf realmName = "TW" Then
            requestURL = "http://m.us.mobileservice.blizzard.com/enrollment/enroll.htm"
        ElseIf realmName = "US" Then
            requestURL = "http://m.us.mobileservice.blizzard.com/enrollment/enroll.htm"
        ElseIf realmName = "EU" Then
            requestURL = "http://m.eu.mobileservice.blizzard.com/enrollment/enroll.htm"
        Else
            'ERROR!!
        End If

        ReDim postData.rLocation(2) '!!
        postData.rLocation = System.Text.Encoding.UTF8.GetBytes(realmName)

        ReDim postData.rRand(36)
        ReDim postData.rDevice(15)

        Randomize()
        Dim counter As Long
        For counter = 0 To 36
            postData.rRand(counter) = CInt(Int((255 * Rnd()) + 1))
        Next

        For counter = 0 To 15
            postData.rDevice(counter) = CInt(Int((255 * Rnd()) + 1))
        Next

        'MsgBox(Marshal.SizeOf(postData))

        '----------------------------------
        Dim hRequest As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(requestURL), System.Net.HttpWebRequest)
        hRequest.Method = "post"
        hRequest.ContentType = "application/octet-stream"
        'hRequest.Expect = "100-continue"
        'convert rawdata 2 byte()

        Dim postByte As Byte()
        ReDim postByte(55)
        postData.rHead.CopyTo(postByte, 0)
        postData.rRand.CopyTo(postByte, postData.rHead.Length - 1)
        postData.rLocation.CopyTo(postByte, postData.rHead.Length + postData.rRand.Length - 1)
        postData.rDevice.CopyTo(postByte, postData.rHead.Length + postData.rRand.Length + postData.rLocation.Length - 1)

        'Dim RSA As New RSACryptoServiceProvider(512)
        'Dim pub As String = RSA.ToXmlString(False)
        'Dim pri As String = RSA.ToXmlString(True)
        'Dim rsaExponent As Byte()
        '\ReDim rsaExponent(2) 'OTZ___
        'rsaExponent(0) = 1
        'rsaExponent(1) = 0
        'rsaExponent(2) = 1
        'rsaExponent = hexStr2byteArr("0101")
        '-------------------------------------------
        'Dim pRSA As RSAParameters
        'pRSA.Exponent = rsaExponent
        'pRSA.Modulus = hexStr2byteArr(strPubKey)

        'Dim rsa As New RSACryptoServiceProvider()
        'rsa.ImportParameters(pRSA)

        'Dim encByte As Byte()
        'encByte = rsa.Encrypt(postByte, False) '他说不需要填充，可是填充了也可以返回data
        '‘放弃了。。’再试试’算了吧
        '------------------------------------------
        Dim rsa As New RsaEngine()
        rsa.Init(True, New RsaKeyParameters(False, New Org.BouncyCastle.Math.BigInteger(strPubKey, 16), New Org.BouncyCastle.Math.BigInteger("0101", 16)))
        Dim encByte As Byte() = rsa.ProcessBlock(postByte, 0, postByte.Length)
        MsgBox(encByte.Length)
        'Dim rsa As new RSACryptoServiceProvider()
        'Dim rp As RSAParameters
        'rp = rsa.ExportParameters(False)
        'Dim publicKeyBytes As Byte() = rsa.ExportCspBlob(False)
        'Dim privateKeyBytes As Byte() = rsa.ExportCspBlob(True)
        'Console.WriteLine(Convert.ToBase64String(publicKeyBytes))
        'Console.WriteLine(Convert.ToBase64String(privateKeyBytes))


        Dim postStream = hRequest.GetRequestStream
        postStream.Write(encByte, 0, encByte.Length)
        postStream.Close()

        Dim hRet As System.Net.WebResponse
        hRet = hRequest.GetResponse()
        'Dim retReader As New System.IO.StreamReader(hRet.GetResponseStream)
        Dim retStream As IO.Stream
        retStream = hRet.GetResponseStream

        Dim retByte As Byte()
        ReDim retByte(45 - 1) 'fixed 记得vb以前不用-1啊好像
        retStream.Read(retByte, 0, 45)

        retStream.Close()
        hRet.Close()
        hRequest.Abort()

        '准备写入本地
        'Dim localData As New localData()

        'retByte！！！
        'return data:
        '00-07 server time (Big Endian)
        '08-45 init data encrpyted with our key
        Dim timeData As Byte()
        ReDim timeData(8 - 1)
        Array.Copy(retByte, timeData, 8)
        If BitConverter.IsLittleEndian = True Then '应该是格式的问题，回头可以自己看看
            Array.Reverse(timeData)
        End If
        Dim serverTimeDiff = BitConverter.ToInt64(timeData, 0) - CurrentTime() 'time diff

        Dim encdRetData As Byte()
        ReDim encdRetData(37 - 1)
        Array.Copy(retByte, 8, encdRetData, 0, 37)
        '用刚才post的37位随机数进行XOR解密
        For i As Integer = 0 To 36
            encdRetData(i) = encdRetData(i) Xor postData.rRand(i)
        Next

        Dim secretKey As Byte()
        ReDim secretKey(20 - 1)
        Dim serial As String
        serial = System.Text.Encoding.Default.GetString(encdRetData, 20, 17)
        Array.Copy(encdRetData, secretKey, 20)

        Me.isEnrolled = True
        Me.secretKey = secretKey
        Me.serial = serial
        Me.ServerTimeDiff = serverTimeDiff

        'Return True
        'enroll.locKey = postData.rRand
        Exit Sub

err:
        Me.isEnrolled = False

    End Sub
    Public Sub Sync()
        Dim requestURL As String

        If realmName = "CN" Then
            requestURL = "http://mobile-service.battlenet.com.cn/enrollment/time.htm"
        ElseIf realmName = "TW" Then
            requestURL = "http://m.us.mobileservice.blizzard.com/enrollment/time.htm"
        ElseIf realmName = "US" Then
            requestURL = "http://m.us.mobileservice.blizzard.com/enrollment/time.htm"
        ElseIf realmName = "EU" Then
            requestURL = "http://m.eu.mobileservice.blizzard.com/enrollment/time.htm"
        Else
            'ERROR!!
        End If

        Dim hRequest As System.Net.HttpWebRequest = CType(System.Net.WebRequest.Create(requestURL), System.Net.HttpWebRequest)
        hRequest.Method = "post"
        hRequest.ContentType = "application/octet-stream"


        Dim hRet As System.Net.WebResponse
        hRet = hRequest.GetResponse()
        'Dim retReader As New System.IO.StreamReader(hRet.GetResponseStream)
        Dim retStream As IO.Stream
        retStream = hRet.GetResponseStream

        Dim timeData As Byte()
        ReDim timeData(8 - 1)
        retStream.Read(timeData, 0, 8)

        retStream.Close()
        hRet.Close()
        hRequest.Abort()

        If BitConverter.IsLittleEndian = True Then
            Array.Reverse(timeData)
        End If
        Me.ServerTimeDiff = BitConverter.ToInt64(timeData, 0) - CurrentTime() 'time diff

    End Sub

    Protected Function CurrentTime() As Long
        CurrentTime = Convert.ToInt64((DateTime.UtcNow - New DateTime(1970, 1, 1)).TotalMilliseconds)
    End Function
    Public Function getPercent() As Single
        Dim sMod As Long = (CurrentTime() + ServerTimeDiff) Mod 30000L
        Dim tmp As Single = sMod / 30000L + 0.5
        If tmp > 1 Then
            Return tmp - 1
        Else
            Return tmp
        End If
        '        Dim sMod As Long = (CurrentTime() + ServerTimeDiff) Mod 30000L
        'If sMod <= 150000 Then
        'sMod += 15000
        'Else
        ' sMod -= 15000
        ' End If
        ' Dim tmp As Single = sMod / 30000L
        ' Return tmp
    End Function
    Public Function calcKey() As String
        '准备加密
        Dim hmac As HMac = New HMac(New Sha1Digest())
        hmac.Init(New KeyParameter(secretKey))

        '计算服务器时间，除30秒，四舍五入，30秒变一次
        Dim CodeInterval As Long = (CurrentTime() + ServerTimeDiff) / 30000L
        Dim codeIntervalArray As Byte() = BitConverter.GetBytes(CodeInterval)
        If BitConverter.IsLittleEndian = True Then
            Array.Reverse(codeIntervalArray)
        End If
        '加密
        hmac.BlockUpdate(codeIntervalArray, 0, codeIntervalArray.Length)

        Dim mac As Byte()
        ReDim mac(hmac.GetMacSize() - 1)
        hmac.DoFinal(mac, 0)

        'the last 4 bits of the mac say where the code starts (e.g. if last 4 bit are 1100, we start at byte 12)
        Dim start As Integer = mac(19) And &HF '按位与运算.0F=00001111相当于只保留了后4bit

        'extract those 4 bytes
        Dim bytes As Byte()
        ReDim bytes(4 - 1)

        Array.Copy(mac, start, bytes, 0, 4)
        If BitConverter.IsLittleEndian = True Then
            Array.Reverse(bytes)
        End If
        'Dim bui As Long = BitConverter.ToUInt32(bytes, 0)
        Dim fullcode As UInteger = BitConverter.ToUInt32(bytes, 0) And &H7FFFFFFF

        'we use the last 8 digits of this code in radix 10
        Dim code As String = (fullcode Mod 100000000).ToString("00000000")

        calcKey = code

    End Function

    Private Shared Function hexStr2byteArr(ByVal str As String) As Byte()
        Dim len As Integer = str.Length
        Dim retByte As Byte()
        ReDim retByte(len / 2 - 1)

        For i As Integer = 0 To len - 1 Step 2
            retByte(i / 2) = Convert.ToByte(str.Substring(i, 2), 16)
        Next

        hexStr2byteArr = retByte
    End Function
End Class


