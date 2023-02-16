VERSION 5.00
Begin VB.Form frmInventoryBalance 
   Caption         =   "Form1"
   ClientHeight    =   3135
   ClientLeft      =   60
   ClientTop       =   405
   ClientWidth     =   4680
   LinkTopic       =   "Form1"
   ScaleHeight     =   3135
   ScaleWidth      =   4680
   StartUpPosition =   2  'CenterScreen
   Begin VB.CommandButton Command1 
      Caption         =   "592"
      Height          =   495
      Left            =   3240
      TabIndex        =   0
      Top             =   840
      Width           =   1215
   End
End
Attribute VB_Name = "frmInventoryBalance"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private Sub Command1_Click()
    Shell "\\172.17.71.30\Shareddll\DotNetDll\netcoreapp3.1\WPFERPQuickLauncher_Core.exe" & " " & """Data Source=172.17.71.3; Initial Catalog=URDB0; User id=sa; Password=Urjita1;""" & " " & "592" & " " & "WPFComInventory_Core" & " " & "frmInvBalance"
    
    
    'Shell "D:\WPF\WPFERPQuickLauncher_Core\WPFERPQuickLauncher_Core\WPFERPQuickLauncher_Core\bin\Debug\netcoreapp3.1\WPFERPQuickLauncher_Core.exe" & " " & """Data Source=172.17.71.3; Initial Catalog=URDB0; User id=sa; Password=Urjita1;""" & " " & "592" & " " & "WPFComInventory_Core" & " " & "frmInvBalance"
End Sub
