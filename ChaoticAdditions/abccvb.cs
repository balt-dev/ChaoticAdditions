using GDWeave.Godot;
using GDWeave.Modding;

public class SampleMod : IScriptMod
{
    public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens) {
        throw new NotImplementedException();
    }

    public bool ShouldRun(string path) {
        throw new NotImplementedException();
    }
}

