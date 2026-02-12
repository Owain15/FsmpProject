$b = [System.IO.File]::ReadAllBytes('c:\Users\ojdav\source\repos\FsmpProject\dependencies\libvlc\libvlc.dll')
$o = [System.BitConverter]::ToInt32($b, 60)
$m = [System.BitConverter]::ToUInt16($b, $o + 4)
if ($m -eq 0x014C) { "x86" } elseif ($m -eq 0x8664) { "x64" } elseif ($m -eq 0xAA64) { "ARM64" } else { "Unknown: $m" }
