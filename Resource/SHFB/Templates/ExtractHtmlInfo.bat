@ECHO OFF

REM Step 6.1 - Extract title and keyword index info.  This will also alter the
REM encoding of the files if localizing them for use with the Help 1.x compiler.
"{@SHFBFolder}SandcastleHtmlExtract.exe" /project="{@HTMLHelpName}" /lcid={@LangID} {@ExtractFlags}
