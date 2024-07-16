# Groth16.Net

[![.NET Core CI](https://github.com/stevenportkey/Groth16.Net/actions/workflows/ci.yml/badge.svg)](https://github.com/stevenportkey/Groth16.Net/actions/workflows/ci.yml)

This is a .NET wrapper for the rust library of Groth16 using `Bn254` curve. The rust library can be
found [here](https://github.com/stevenportkey/libgroth16).

## Example Usage:

### Generate a proof

```csharp
var wasmPath = "./multiplier2.wasm";
var r1csPath = "./multiplier2.r1cs";
var zkeyPath = "./multiplier2_0001.zkey";

// Create a Prover instance, make sure to dispose it after use
using var prover = Prover.Create(wasmPath, r1csPath, zkeyPath);

// Prepare the input arguments. Each argument is an array of decimal strings
var provingInput = new Dictionary<string, IList<string>>()
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
    }

// Generate the proof
var provingOutputString = prover.ProveBn254(provingInput);
```

The provingOutputString will be something like:

```json
{
  "public_inputs": [
    "33"
  ],
  "proof": {
    "pi_a": [
      "280975432816980108679759071487297601107108018820257681079318024398651794004",
      "4205131994626031290459080602672329937446907135687633563592593691710690566112",
      "1"
    ],
    "pi_b": [
      [
        "17469063948367491524572656383512471181189921004923303439702704901345039573517",
        "9714352188110531115047659652714278896321447089768041035415280669957375168144"
      ],
      [
        "10132013297607185549235803992203008010610955622347160100279643061981310934576",
        "10382741250397028266948880323413119705541540222284447240724605330446620596999"
      ],
      [
        "1",
        "0"
      ]
    ],
    "pi_c": [
      "3387378824708952661725418937794852739053540574512261821587479411220237725353",
      "21786624385058368801090727068086049405483536493453442106165045865639697478470",
      "1"
    ],
    "protocol": "groth16"
  }
}
```

### Verify

```csharp
var verifyingKey =
    "67d28bc9637e5842e652e8d19b3c87413c8242b1607c7e738ac2f5d435248d18f68f2792009ed932a3c7f400c2ba7cd9181f331226134fa75dc2cee1e667a3241fca13b5eb1c3b310900c6525fb76657be52dfdf3db1132d0223bee44219a219edf692d95cbdde46ddda5ef7d422436779445c5e66006a42761e1f12efde0018c212f3aeb785e49712e7a9353349aaf1255dfb31b7bf60723a480d9293938e199b5db8c426b2f2dde0734aa84d0fc9c6ac0b22e89221a030f925eb84996267128258040574d428374aaaaeebb617b4a3f605551973446a56006d973837825ea60200000000000000afcf5144f601ac362d46821e817ebd51e4a25e403862b6796606e18d2b45be0c519f16ffda911e05d663e81541ff4fa1e5eeaf4d2803d9698ad9f967177212a7";
var publicInputs = new List<string>() { "33" };
var proof = new RapidSnarkProof()
{
    PiA = new List<string>()
    {
        "280975432816980108679759071487297601107108018820257681079318024398651794004",
        "4205131994626031290459080602672329937446907135687633563592593691710690566112",
        "1"
    },
    PiB = new List<List<string>>()
    {
        new List<string>()
        {
            "17469063948367491524572656383512471181189921004923303439702704901345039573517",
            "9714352188110531115047659652714278896321447089768041035415280669957375168144"
        },
        new List<string>()
        {
            "10132013297607185549235803992203008010610955622347160100279643061981310934576",
            "10382741250397028266948880323413119705541540222284447240724605330446620596999"
        },
        new List<string>()
        {
            "1", "0"
        }
    },
    PiC = new List<string>()
    {
        "3387378824708952661725418937794852739053540574512261821587479411220237725353",
        "21786624385058368801090727068086049405483536493453442106165045865639697478470",
        "1"
    }
};
var valid = Verifier.VerifyBn254(verifyingKey, publicInputs, proof);
```
