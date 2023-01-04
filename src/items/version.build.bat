"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\TextTransform.exe" -a !!build!true "version.build.tt"
@echo off
for /f "tokens=*" %%s in (version.build.props) do (
  echo %%s
)
pause

