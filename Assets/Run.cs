using System;
using System.IO;
using UnityEngine;
using WebAssembly; // Acquire from https://www.nuget.org/packages/WebAssembly
using WebAssembly.Runtime;

// We need this later to call the code we're generating.
public abstract class WasmInterface
{
    // Sometimes you can use C# dynamic instead of building an abstract class like this.
    public abstract void main();
}

public class Run : MonoBehaviour
{
    public TextAsset moduleFile;
    private Instance<WasmInterface> instance;

    void Start()
    {
        Module module = Module.ReadFromBinary(new MemoryStream(moduleFile.bytes));

        var instanceCreator = module.Compile<WasmInterface>();

        // read(componentIndex)

        var memory = new UnmanagedMemory(10000, 1000000);

        instance = instanceCreator(new ImportDictionary {
                    { "env", "memory", new MemoryImport(() => memory) },
                    { "unity", "read", new FunctionImport(new Func<float>(() => transform.position.x)) },
                    { "unity", "write", new FunctionImport(new Action<float>((x) =>
                    {
                        transform.position = new Vector3(x, transform.position.y, transform.position.z);
                        Debug.Log(x);
                    } ))}
            });
    }

    void Update()
    {
        instance.Exports.main();
    }

    private void OnDestroy()
    {
        instance.Dispose();
    }
}
