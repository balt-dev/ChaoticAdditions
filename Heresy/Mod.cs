using GDWeave;

namespace Heresy;

public class Mod : IMod {
    public Config Config;

    public Mod(IModInterface modInterface) {
        this.Config = modInterface.ReadConfig<Config>();
        modInterface.RegisterScriptMod(new HeresyMod(Config, modInterface.Logger));
    }

    public void Dispose() {
        // Cleanup anything you do here
    }
}
