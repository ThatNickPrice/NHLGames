﻿Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports NHLGames.My.Resources

Namespace Utilities
    Public Class Common

        Public Const UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36"
        Private Const Http = "http"
        Private Const Timeout = 2000

        Public Shared Function GetRandomString(ByVal intLength As Integer)
            Const s As String = "abcdefghijklmnopqrstuvwxyz0123456789"
            Dim r As New Random
            Dim sb As New Text.StringBuilder
            For i = 1 To intLength
                Dim idx As Integer = r.Next(0, 35)
                sb.Append(s.Substring(idx, 1))
            Next

            Return sb.ToString()
        End Function
        
        Public Shared Function SetHttpWebRequest(address As String) As HttpWebRequest
            Dim defaultHttpWebRequest As HttpWebRequest = CType(WebRequest.Create(New Uri(address)), HttpWebRequest)
            defaultHttpWebRequest.UserAgent = UserAgent
            defaultHttpWebRequest.Method = WebRequestMethods.Http.Get
            defaultHttpWebRequest.Proxy = Nothing
            defaultHttpWebRequest.ContentType = "text/plain"
            defaultHttpWebRequest.CookieContainer = New CookieContainer()
            defaultHttpWebRequest.CookieContainer.Add(New Cookie("mediaAuth", Common.GetRandomString(240), String.Empty, "nhl.com"))
            defaultHttpWebRequest.Timeout = Timeout
            Return defaultHttpWebRequest
        End Function

        Public Shared Function SendWebRequest(ByVal address As String, Optional httpWebRequest As HttpWebRequest = Nothing) As Boolean
            If address Is Nothing AndAlso httpWebRequest Is Nothing Then Return False
            Dim myHttpWebRequest As HttpWebRequest
            If httpWebRequest Is Nothing Then
                myHttpWebRequest = SetHttpWebRequest(address)
            Else 
                myHttpWebRequest = httpWebRequest
            End If
            Try
                Dim myHttpWebResponse As HttpWebResponse = myHttpWebRequest.GetResponse()
                If myHttpWebResponse.StatusCode = HttpStatusCode.OK Then
                    Return True
                End If
                myHttpWebResponse.Close()
            Catch ex As Exception
                Return False
            End Try
            Return False
        End Function

        Public Shared Async Function SendWebRequestAsync(ByVal address As String, Optional httpWebRequest As HttpWebRequest = Nothing) As Task(Of Boolean)
            If address Is Nothing AndAlso httpWebRequest Is Nothing Then Return False
            Dim myHttpWebRequest As HttpWebRequest
            If httpWebRequest Is Nothing Then
                myHttpWebRequest = SetHttpWebRequest(address)
            Else 
                myHttpWebRequest = httpWebRequest
            End If
            Try
                Using myHttpWebResponse As HttpWebResponse = Await myHttpWebRequest.GetResponseAsync()
                    If myHttpWebResponse.StatusCode = HttpStatusCode.OK Then
                        Return True
                    End If
                End Using
            Catch
                Return False
            End Try
            Return False
        End Function

        Public Shared Function SendWebRequestAndGetContent(ByVal address As String, Optional httpWebRequest As HttpWebRequest = Nothing) As String
            Dim content = New MemoryStream()
            Dim myHttpWebRequest As HttpWebRequest
            If httpWebRequest Is Nothing Then
                myHttpWebRequest = SetHttpWebRequest(address)
            Else 
                myHttpWebRequest = httpWebRequest
            End If
            Try
                Dim myHttpWebResponse As WebResponse = myHttpWebRequest.GetResponse()
                Dim reader As Stream = myHttpWebResponse.GetResponseStream()
                reader.CopyTo(content)
                reader.Close()
                myHttpWebResponse.Close()
            Catch ex As Exception
                content.Dispose()
                Return String.Empty
            End Try
            Return System.Text.Encoding.UTF8.GetString(content.ToArray())
        End Function

        Public Shared Async Function GetContent(ByVal address As String, ByVal client As HttpClient) As Task(of String)
            Try
                Dim response As HttpResponseMessage = Await client.GetAsync(address)
                If response.StatusCode <> HttpStatusCode.OK Then Return String.Empty
                Dim content As HttpContent = response.Content
                Dim result As String = Await content.ReadAsStringAsync()
                content.Dispose()
                response.Dispose()
                Return result
            Catch ex As Exception
                Return String.Empty
            End Try
        End Function

        Public Shared Async Function SendWebRequestAndGetContentAsync(ByVal address As String, Optional httpWebRequest As HttpWebRequest = Nothing) As Task(Of String)
            Dim content = New MemoryStream()
            Dim myHttpWebRequest As HttpWebRequest
            If httpWebRequest Is Nothing Then
                myHttpWebRequest = SetHttpWebRequest(address)
            Else 
                myHttpWebRequest = httpWebRequest
            End If
            Try
                Dim myHttpWebResponse As WebResponse = Await myHttpWebRequest.GetResponseAsync()
                Dim reader As Stream = myHttpWebResponse.GetResponseStream()
                reader.CopyTo(content)
                reader.Close()
                myHttpWebResponse.Close()
            Catch
                content.Dispose()
                Return String.Empty
            End Try
            Return System.Text.Encoding.UTF8.GetString(content.ToArray())
        End Function

        Public Shared Sub GetLanguage()
            Dim lang = ApplicationSettings.Read(Of String)(SettingsEnum.SelectedLanguage, String.Empty)
            If String.IsNullOrEmpty(lang) OrElse lang = NHLGamesMetro.RmText.GetString("lblEnglish") Then
                NHLGamesMetro.RmText = English.ResourceManager
            ElseIf lang = NHLGamesMetro.RmText.GetString("lblFrench") Then
                NHLGamesMetro.RmText = French.ResourceManager
            End If
        End Sub

        Public Shared Function CheckAppCanRun() As Boolean
            If Not File.Exists("NHLGames.exe.Config") Then
                FatalError(NHLGamesMetro.RmText.GetString("noConfigFile"))
                Return False
            Else If Not InitializeForm.VersionCheck() OrElse Not SendWebRequest("https://www.google.com") Then
                FatalError(NHLGamesMetro.RmText.GetString("noWebAccess"))
                Return False
            End If
            Return True
        End Function

        Private Shared Sub FatalError(message As String)
            If InvokeElement.MsgBoxRed(message, NHLGamesMetro.RmText.GetString("msgFailure"), MessageBoxButtons.OK) = DialogResult.OK Then
                NHLGamesMetro.FormInstance.Close
            End If
        End Sub

    End Class
End Namespace
