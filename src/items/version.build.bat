"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\TextTransform.exe" "version.build.tt" -a !!build!true
@echo off
for /f "tokens=*" %%s in (version.build.props) do (
  echo %%s
)
pause

