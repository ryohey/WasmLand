using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
 using WebAssembly; // Acquire from https://www.nuget.org/packages/WebAssembly
using WebAssembly.Runtime;

// We need this later to call the code we're generating.
public abstract class WasmInterface
{
    // Sometimes you can use C# dynamic instead of building an abstract class like this.
    public abstract int main();
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
                    }
                    ))}
        });

        var ptr = instance.Exports.main();
        Debug.Log(ptr);
        var str = ReadString(memory, ptr);
         Debug.Log(str);
        CallMethod(str);

        // TODO: Call C# method from WebAssembly 
    }

    string ReadString(UnmanagedMemory memory, int ptr)
    {
        var str = "";

        for (var i = 0; true; i++)
        {
            var v = Marshal.ReadInt32(memory.Start + ptr + i * 4);
            if (v == 0)
            {
                break;
            }
            str += (char)v;
        }
        return str;
    }

    void CallMethod(string signature)
    {
          
    }

    void Update()
    {
    }

    private void OnDestroy()
    {
        instance.Dispose();
    }
}
