rm pkg.zip
mkdir -p pkg/GDWeave/mods/baltdev.Heresy
cd Heresy
dotnet build -c:Release -o:../bin
cd ..
cp bin pkg/GDWeave/mods/baltdev.Heresy/bin -r
cp manifest.json pkg
cp manifest.json pkg/GDWeave/mods/baltdev.Heresy
cp icon.png pkg
cp icon.png pkg/GDWeave/mods/baltdev.Heresy
cp README.md pkg
cd pkg
zip ../pkg.zip ./* -r
cd ..
rm -rf pkg
