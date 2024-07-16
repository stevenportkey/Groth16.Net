using Newtonsoft.Json;

namespace Groth16.Net.Tests;

public class RapidSnarkProofAdaptor
{
    [JsonProperty("pi_a")] public List<string> PiA { get; set; }
    [JsonProperty("pi_b")] public List<List<string>> PiB { get; set; }
    [JsonProperty("pi_c")] public List<string> PiC { get; set; }
}

public class ProvingOutput
{
    public ProvingOutput(IList<string> publicInputs, RapidSnarkProofAdaptor proof)
    {
        PublicInputs = publicInputs;
        Proof = proof;
    }

    [JsonProperty("public_inputs")] public IList<string> PublicInputs { get; set; }

    [JsonProperty("proof")] public RapidSnarkProofAdaptor Proof { get; set; }

    public static ProvingOutput FromJsonString(string jsonString)
    {
        return JsonConvert.DeserializeObject<ProvingOutput>(jsonString);
    }
}

public class Groth16Tests
{
    private IDictionary<string, IList<string>> ProvingInput = new Dictionary<string, IList<string>>()
    {
        {
            "a", new List<string>
            {
                "3"
            }
        },
        {
            "b", new List<string>
            {
                "11"
            }
        }
    };

    string ExpectedInputString = "{\"a\":[\"3\"],\"b\":[\"11\"]}";

    [Fact]
    public void Test_InputSerialization()
    {
        var inputString = ProvingInput.ToJsonString();
        Assert.Equal(ExpectedInputString, inputString);
    }

    [Fact]
    public void Test_ProveAndVerify()
    {
        var wasmPath = "../../../data-files/multiplier2.wasm";
        var r1csPath = "../../../data-files/multiplier2.r1cs";
        var zkeyPath = "../../../data-files/multiplier2_0001.zkey";
        using var prover = Prover.Create(wasmPath, r1csPath, zkeyPath);

        var provingOutputString = prover.ProveBn254(ProvingInput);
        var provingOutput = ParseProvingOutput(provingOutputString);
        var verified = Verifier.VerifyBn254(prover.ExportVerifyingKeyBn254(), provingOutput.PublicInputs,
            new RapidSnarkProof
            {
                PiA = provingOutput.Proof.PiA,
                PiB = provingOutput.Proof.PiB,
                PiC = provingOutput.Proof.PiC
            });
        Assert.True(verified);
    }

    [Fact]
    public void Test_ContextCreateFails_InvalidPath()
    {
        var wasmPath = "/ThisIsAnInvalidFile.wasm";
        var r1csPath = "/ThisIsAnInvalidFile.r1cs";
        var zkeyPath = "/ThisIsAnInvalidFile_0001.zkey";
        Assert.Throws<FileNotFoundException>(() => Prover.Create(wasmPath, r1csPath, zkeyPath));
    }

    private static ProvingOutput ParseProvingOutput(string provingOutput)
    {
        var provingOutputObj = JsonConvert.DeserializeObject<ProvingOutput>(provingOutput);
        return provingOutputObj;
    }
}