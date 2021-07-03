﻿Imports System.IO

Public Class Form2
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Label13.Visible = True
        Label14.Visible = True
        Label15.Visible = True
        Label16.Visible = True
        TextBox10.Visible = True
        TextBox11.Visible = True
        TextBox12.Visible = True
        TextBox13.Visible = True

        'Si antigo não tem data sheet
        If ComboBox1.Text.Equals("Si antigo") Then
            Button1.Visible = False
        Else
            Button1.Visible = True
        End If

        'validar que os LN6N tenham essa mensagem
        If ComboBox1.Text.Equals("Si antigo") Or ComboBox1.Text.Equals("EOS Si S-type detector S-series") Then

        Else
            MsgBox("Este é um sensor LN6N. Lembre-se de sempre resfria-lo com NL antes de liga-lo na fonte de alimentação")
        End If


        Dim sensor = New SensorFabricante(ComboBox1.Text)
        TextBox10.Text = sensor.Material
        TextBox11.Text = sensor.Area
        TextBox12.Text = sensor.RespMax
        TextBox13.Text = sensor.FaixaEspectral

        'retira o " mm²" do final.
        TextBox3.Text = sensor.Area.Substring(0, sensor.Area.Length - 4)



    End Sub

    Private Sub ButtonCalibracao_Click(sender As Object, e As EventArgs) Handles ButtonCalibracao.Click
        GlobalVariables.OpenFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        GlobalVariables.OpenFileDialog1.Title = "Buscando arquivo..."
        GlobalVariables.OpenFileDialog1.Filter = "Text Files|*.txt;*.doc;*.med;*.ref|All files|*.*" 'med e ref sao formatos que saem os arquivos do programa principal
        GlobalVariables.OpenFileDialog1.RestoreDirectory = True
        Dim DidWork As Integer = GlobalVariables.OpenFileDialog1.ShowDialog()
        If DidWork = DialogResult.Cancel Then
            MessageBox.Show("Você cancelou a abertura")
            TextBox1.Text = "Fail !"
            TextBox1.BackColor = Color.Red
            Exit Sub 'isso faz com que saia do evento de botão clido, ele suspende todas as ações posteriores

        Else
            TextBox1.Text = "Inserido !"
            TextBox1.BackColor = Color.SpringGreen

        End If

    End Sub
    Private Sub ButtonAmostra_Click(sender As Object, e As EventArgs) Handles ButtonAmostra.Click
        GlobalVariables.OpenFileDialog2.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        GlobalVariables.OpenFileDialog2.Title = "Buscando arquivo..."
        GlobalVariables.OpenFileDialog2.Filter = "Text Files|*.txt;*.doc;*.med;*.ref|All files|*.*" 'med e ref sao formatos que saem os arquivos do programa principal
        GlobalVariables.OpenFileDialog2.RestoreDirectory = True
        Dim DidWork As Integer = GlobalVariables.OpenFileDialog2.ShowDialog()
        If DidWork = DialogResult.Cancel Then
            MessageBox.Show("Você cancelou a abertura")
            TextBox2.Text = "Fail !"
            TextBox2.BackColor = Color.Red
            Exit Sub 'isso faz com que saia do evento de botão clido, ele suspende todas as ações posteriores
        Else
            TextBox2.Text = "Inserido !"
            TextBox2.BackColor = Color.SpringGreen
        End If

    End Sub

    Public Class GlobalVariables
        Public Shared OpenFileDialog1 As New OpenFileDialog
        Public Shared OpenFileDialog2 As New OpenFileDialog
        Public Shared FolderBrowserDialog1 As New FolderBrowserDialog
        Public Shared pathNewArchive As String
        Public Shared nameNewArchive As String

    End Class

    Private Sub ButtonCalcular_Click(sender As Object, e As EventArgs) Handles ButtonCalcular.Click
        Dim PathRef As String
        PathRef = ComboBox1.Text
        Dim filePath As String = IO.Path.Combine(Application.StartupPath, "TxtsDasReferencias", PathRef + ".txt") 'aqui está pegando o caminho (interno) do sensor de referencia escolhido dentre as opções

        Dim sensor = New SensorFabricante(ComboBox1.Text)
        'retira o " mm²" do final
        TextBox3.Text = sensor.Area.Substring(0, sensor.Area.Length - 4)

        'Parâmetros para o alfa (coeficiente que corrige os valores por causa das diferentes áreas e distancias)
        Dim areaSensorDeReferencia As Double
        Dim distanciaSensorDeReferencia As Double
        Dim areaAmostra As Double
        Dim distanciaAmostra As Double
        Dim sucesso As Boolean

        'variavel booleana que verifica se os valores dos parametros de correção foram inseridos corretamente
        sucesso = Double.TryParse(TextBox3.Text, areaSensorDeReferencia) And Double.TryParse(TextBox4.Text, distanciaSensorDeReferencia) And Double.TryParse(TextBox5.Text, areaAmostra) And Double.TryParse(TextBox6.Text, distanciaAmostra)
        'Caso qualquer um dos parâmetros inseridos for zero, dará mensagem de erro
        If (areaSensorDeReferencia = 0 Or distanciaSensorDeReferencia = 0 Or areaAmostra = 0 Or distanciaAmostra = 0) Then
            MsgBox("Parâmetro com valor ZERO não existe. Coloque um valor válido e continue.")
            Exit Sub 'isso faz com que saia do evento de botão clido, ele suspende todas as ações posteriores
        End If
        Dim alfa As Double
        If (sucesso) Then
            alfa = (areaSensorDeReferencia / areaAmostra) * (distanciaAmostra ^ 2 / distanciaSensorDeReferencia ^ 2)
        Else
            alfa = 1 'se faltar algum parametro para o cálculo, deixa o alfa = 1 para poder rodar o programa nesse caso específico.
        End If

        Dim textRef As String 'variável para pegar os dados do txt em forma de string
        Dim textCalib As String 'variável para pegar os dados do txt em forma de string
        Dim textAmostra As String 'variável para pegar os dados do txt em forma de string

        Dim indice As Integer
        Dim temporario As Double
        Dim yInterpolado(0) As Double

        Dim potencia(0) As Double
        Dim responsividade(0) As Double

        Dim columnXRef() As Double
        Dim columnYRef() As Double

        Dim columnXCalib() As Double
        Dim columnYCalib() As Double

        Dim columnXAmostra() As Double
        Dim columnYAmostra() As Double

        'curva do sensor de referencia dado pelo fabricante
        textRef = readTxtComplete(filePath) 'filepath é o caminho do arquivo do sensor de referencia dado pelo fabricante
        columnYRef = getColumYOfStringComplete(textRef) 'column y é a primeira coluna do txt
        columnXRef = getColumXOfStringComplete(textRef)  'column x é a segunda coluna do txt
        'curva do sensor de referencia no SETUP
        textCalib = readTxtComplete(GlobalVariables.OpenFileDialog1.FileName) 'aqui ele vai pegar o caminho do arquivo 
        columnYCalib = getColumYOfStringComplete(textCalib)
        columnXCalib = getColumXOfStringComplete(textCalib)
        'curva da amostra no SETUP
        textAmostra = readTxtComplete(GlobalVariables.OpenFileDialog2.FileName)
        columnYAmostra = getColumYOfStringComplete(textAmostra)
        columnXAmostra = getColumXOfStringComplete(textAmostra)


        Dim menor As Integer = 0
        If columnXCalib(0) > columnXRef(0) Then 'se o menor valor do Xcalib ta dentro da referencia, entao começa do zero
            menor = 0
        Else
            While columnXCalib(menor) < columnXRef(0)
                menor += 1
            End While 'menor será o menor indice valido do Xcalib
        End If


        Dim maior As Integer = columnXCalib.Length - 1
        If columnXCalib(columnXCalib.Length - 1) < columnXRef(columnXRef.Length - 1) Then
            maior = columnXCalib.Length - 1
        Else
            While columnXCalib(maior) > columnXRef(columnXRef.Length - 1)
                maior -= 1
            End While 'maior é o ultimo indice valido de Xcalib
        End If


        'filtrando os vetores calib e amostra para valores só dentro do range do fabricante
        Dim NewColumnXCalib = columnXCalib.Take(maior + 1).Skip(menor).ToArray()
        Dim NewColumnYCalib = columnYCalib.Take(maior + 1).Skip(menor).ToArray()

        Dim NewColumnXAmostra = columnXAmostra.Take(maior + 1).Skip(menor).ToArray()
        Dim NewColumnYAmostra = columnYAmostra.Take(maior + 1).Skip(menor).ToArray()

        'Em posse de todos os vetores, agora calcularemos a responsividade
        For i = 0 To NewColumnXCalib.Length - 1
            indice = menorque(NewColumnXCalib(i), columnXRef)

            temporario = ((NewColumnXCalib(i)) * (columnYRef(indice + 1) - columnYRef(indice)) + columnYRef(indice) * columnXRef(indice + 1) - columnXRef(indice) * columnYRef(indice + 1)) / (columnXRef(indice + 1) - columnXRef(indice))
            Add(Of Double)(yInterpolado, temporario)

            If temporario.Equals(0) Then
                Add(Of Double)(potencia, 0)
            Else
                Add(Of Double)(potencia, NewColumnYCalib(i) / temporario)
            End If
            If (NewColumnYCalib(i).Equals(0)) Then
                Add(Of Double)(responsividade, 0)
            Else
                Add(Of Double)(responsividade, (NewColumnYAmostra(i) * temporario * alfa) / NewColumnYCalib(i))
            End If

        Next i
        Array.Resize(responsividade, responsividade.Length - 1) 'a funcao add sempre deixa o ultimo lugar vago, tira-se entao

        'aqui começa 
        Dim sfdPic As New SaveFileDialog()
        Dim initialDirectory As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)

        Try

            With sfdPic
                .Title = "Salve o arquivo como"
                .Filter = "txt|*.txt"
                .AddExtension = True
                .DefaultExt = ".txt"
                .FileName = "Arquivo_RESP.txt"
                .ValidateNames = True
                .OverwritePrompt = True
                .InitialDirectory = initialDirectory
                .RestoreDirectory = True

                If .ShowDialog = DialogResult.OK Then
                    Dim outFile As IO.StreamWriter
                    Dim qlqr As String
                    Dim cabecalho As String
                    Dim datahoraAtual As DateTime = Now
                    cabecalho = "ARQUIVO DE MEDIDA DE RESPONSIVIDADE" & vbCrLf &
                        "Usuário: " & "" &
                        "Amostra: " & "" & vbCrLf &
                        "Data e Hora: " & datahoraAtual.ToShortDateString & " " & datahoraAtual.ToShortTimeString & vbCrLf &
                        "Magnitude (mV)	Comprimento de onda(nm)"
                    outFile = My.Computer.FileSystem.OpenTextFileWriter(sfdPic.FileName, True)
                    outFile.WriteLine(cabecalho)
                    For k = 0 To responsividade.Length - 1
                        qlqr = responsividade(k).ToString + " " + NewColumnXAmostra(k).ToString
                        outFile.WriteLine(qlqr)
                    Next k
                    outFile.Close()
                Else
                    Return
                End If

            End With

            Dim r As DialogResult
            Dim msg As String = "O arquivo foi salvo corretamente." & vbNewLine
            msg &= "Você quer abrir o arquivo agora?"

            Dim title As String = "Abrir o arquivo."
            Dim btn = MessageBoxButtons.YesNo
            Dim ico = MessageBoxIcon.Information

            r = MessageBox.Show(msg, title, btn, ico)

            If r = System.Windows.Forms.DialogResult.Yes Then
                Dim startInfo As New ProcessStartInfo("notepad.exe")
                startInfo.WindowStyle = ProcessWindowStyle.Maximized
                startInfo.Arguments = sfdPic.FileName
                Process.Start(startInfo)
            Else
                Return
            End If

        Catch ex As Exception
            MessageBox.Show("Erro: Salvar o arquivo falhou ->> " & ex.Message.ToString())
        Finally
            sfdPic.Dispose()
        End Try
        Me.Hide()
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim PathRef As String
        PathRef = ComboBox1.Text
        Dim filePath As String = IO.Path.Combine(Application.StartupPath, "TxtsDasReferencias", PathRef + ".pdf")
        Process.Start(filePath)
    End Sub
End Class