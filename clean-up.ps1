Write-Output "Cleaning up the environment"

# Deepgram
# Remove-Item -Recurse -Force
Remove-Item -Recurse -Force -LiteralPath "./.vs"
Remove-Item -Recurse -Force -LiteralPath "./dist"
Remove-Item -Recurse -Force -LiteralPath "./Deepgram/obj"
Remove-Item -Recurse -Force -LiteralPath "./Deepgram/bin"

#Deepgram.Tests
Remove-Item -Recurse -Force -LiteralPath "./Deepgram.Tests/bin"
Remove-Item -Recurse -Force -LiteralPath "./Deepgram.Tests/obj"

#Deepgram.Microphone
Remove-Item -Recurse -Force -LiteralPath "./Deepgram.Microphone/bin"
Remove-Item -Recurse -Force -LiteralPath "./Deepgram.Microphone/obj"
