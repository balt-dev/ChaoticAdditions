using GDWeave;

namespace ChaoticAdditions;

public class Mod : IMod {
    public Config Config;

    public Mod(IModInterface modInterface) {
        this.Config = modInterface.ReadConfig<Config>();
        modInterface.RegisterScriptMod(new ExampleScriptMod(Config, modInterface.Logger));
        
        modInterface.Logger.Information(":3");
    }

    public void Dispose() {
        // Cleanup anything you do here
    }
}
