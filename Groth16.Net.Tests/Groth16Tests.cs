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

    private string SampleProvingOutput = @"{
                    ""public_inputs"": [
                        ""33""
                    ],
                    ""proof"": {
                        ""pi_a"": [
                            ""12336695469156018292481141270517996964696773355370841935024890155014968932527"",
                            ""8044045689517669816573620068881201435276359727455227997552586962939332812774"",
                            ""1""
                        ],
                        ""pi_b"": [
                            [
                                ""13758076896856520274996566571094284496158660837978144246898599080784571435670"",
                                ""10540243651082236412720548708582495279406122878736723620887101184785423637436""
                            ],
                            [
                                ""3399315555206214103595647914620077356738439777378528796197282998234266434092"",
                                ""12026970324428135854953741176541641386384869229103912517052498071802678655568""
                            ],
                            [
                                ""1"",
                                ""0""
                            ]
                        ],
                        ""pi_c"": [
                            ""21789355289092986944973522794856318156645432436977114401689794498274710366636"",
                            ""9341374468799232211052577304325801081056622027189098336787887092187917383704"",
                            ""1""
                        ],
                        ""protocol"": ""groth16""
                    }
                }";

    private string VerifyingKey =
        "67d28bc9637e5842e652e8d19b3c87413c8242b1607c7e738ac2f5d435248d18f68f2792009ed932a3c7f400c2ba7cd9181f331226134fa75dc2cee1e667a3241fca13b5eb1c3b310900c6525fb76657be52dfdf3db1132d0223bee44219a219edf692d95cbdde46ddda5ef7d422436779445c5e66006a42761e1f12efde0018c212f3aeb785e49712e7a9353349aaf1255dfb31b7bf60723a480d9293938e199b5db8c426b2f2dde0734aa84d0fc9c6ac0b22e89221a030f925eb84996267128258040574d428374aaaaeebb617b4a3f605551973446a56006d973837825ea60200000000000000afcf5144f601ac362d46821e817ebd51e4a25e403862b6796606e18d2b45be0c519f16ffda911e05d663e81541ff4fa1e5eeaf4d2803d9698ad9f967177212a7";

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
    public void Test_ProvingFails_Invalid_Input()
    {
        var wasmPath = "../../../data-files/multiplier2.wasm";
        var r1csPath = "../../../data-files/multiplier2.r1cs";
        var zkeyPath = "../../../data-files/multiplier2_0001.zkey";
        using var prover = Prover.Create(wasmPath, r1csPath, zkeyPath);
        var invalidInput = new Dictionary<string, IList<string>>()
        {
            {
                "a", new List<string>
                {
                    "ab"
                }
            }
        };

        Assert.Throws<ArgumentException>(() => prover.ProveBn254(invalidInput));
    }

    [Fact]
    public void Test_ContextCreateFails_InvalidPath()
    {
        var wasmPath = "/ThisIsAnInvalidFile.wasm";
        var r1csPath = "/ThisIsAnInvalidFile.r1cs";
        var zkeyPath = "/ThisIsAnInvalidFile_0001.zkey";
        Assert.Throws<FileNotFoundException>(() => Prover.Create(wasmPath, r1csPath, zkeyPath));
    }


    [Fact]
    public void Test_Verify()
    {
        var provingOutput = ParseProvingOutput(SampleProvingOutput);
        var verified = Verifier.VerifyBn254(VerifyingKey, provingOutput.PublicInputs,
            new RapidSnarkProof
            {
                PiA = provingOutput.Proof.PiA,
                PiB = provingOutput.Proof.PiB,
                PiC = provingOutput.Proof.PiC
            });
        Assert.True(verified);
    }


    [Fact]
    public void Test_Verify_InvalidInputs()
    {
        var tweaks = new List<Action<RapidSnarkProof>>()
        {
            p => p.PiA.RemoveAt(0),
            p => p.PiA[2] = "2",
            p => p.PiA[0] = "abc",
            p => p.PiB.RemoveAt(0),
            p => p.PiB[2][0] = "2",
            p => p.PiB[0][0] = "abc",
            p => p.PiB[2][1] = "1",
            p => p.PiC.RemoveAt(0),
            p => p.PiC[2] = "2",
            p => p.PiC[0] = "abc",
        };
        foreach (var tweak in tweaks)
        {
            var provingOutput = ParseProvingOutput(SampleProvingOutput);
            var proof = new RapidSnarkProof
            {
                PiA = provingOutput.Proof.PiA,
                PiB = provingOutput.Proof.PiB,
                PiC = provingOutput.Proof.PiC
            };
            tweak(proof);
            Assert.Throws<InvalidProofFormatException>(() =>
                Verifier.VerifyBn254(VerifyingKey, provingOutput.PublicInputs, proof));
        }
    }

    private static ProvingOutput ParseProvingOutput(string provingOutput)
    {
        var provingOutputObj = JsonConvert.DeserializeObject<ProvingOutput>(provingOutput);
        return provingOutputObj;
    }
}