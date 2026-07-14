# KeyViz (Key Visualizer)

## ติดตั้งและรัน

รองรับ Windows 10/11 และต้องติดตั้ง [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```text
# เปิด Command Prompt, PowerShell หรือ terminal อื่นที่โฟลเดอร์โปรเจกต์ แล้วรัน

dotnet restore
dotnet build
dotnet run

# โปรแกรมจะอยู่ใน System Tray คลิกขวาที่ไอคอนเพื่อซ่อน แสดง หรือปิดโปรแกรม
# สร้างไฟล์สำหรับนำไปใช้เครื่องอื่น

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# ไฟล์ที่ publish แล้วอยู่ที่

bin\Release\net10.0-windows\win-x64\publish\KeyViz.exe

# คัดลอก `KeyViz.exe` ไปยังเครื่อง Windows 10/11 แล้วเปิดใช้งานได้ทันที ไม่ต้องติดตั้ง .NET เพิ่ม
```

---

## Install and run

KeyViz supports Windows 10/11 and requires the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```text
# Open Command Prompt, PowerShell, or another terminal in the project directory, then run

dotnet restore
dotnet build
dotnet run

# KeyViz runs in the System Tray. Right-click its icon to show, hide, or exit the application.
# Build a file for use on another machine

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# The published file is located at

bin\Release\net10.0-windows\win-x64\publish\KeyViz.exe

# Copy `KeyViz.exe` to a Windows 10/11 machine and run it directly. No additional .NET installation is required.
```
