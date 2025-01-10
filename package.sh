rm pkg.zip
mkdir pkg
cd Heresy
dotnet build -c:Release -o:../bin
cd ..
cp bin pkg/bin -r
cp manifest.json pkg
cp icon.png pkg
cd pkg
zip ../pkg.zip ./* -r
cd ..
rm -rf pkg