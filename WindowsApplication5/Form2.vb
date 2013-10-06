Public Class Form2

    Private Sub RadioButton1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton1.CheckedChanged
        Form1.ChangeRealm(0)
    End Sub

    Private Sub RadioButton2_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton2.CheckedChanged
        Form1.ChangeRealm(1)
    End Sub

    Private Sub RadioButton3_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton3.CheckedChanged
        Form1.ChangeRealm(2)
    End Sub

    Private Sub RadioButton4_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButton4.CheckedChanged
        Form1.ChangeRealm(3)
    End Sub


    Private Sub Form2_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Select Case Form1.CurrentRealm
            Case 0
                RadioButton1.Checked = True
            Case 1
                RadioButton2.Checked = True
            Case 2
                RadioButton3.Checked = True
            Case 3
                RadioButton4.Checked = True

        End Select
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Form1.sRealm(Form1.CurrentRealm).Sync()
        realm.SaveInfo(Form1.sRealm, 4)
        Form1.ChangeRealm(Form1.CurrentRealm)
    End Sub
End Class