using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class LightSettings
{
    // Fields
    // public const string PixelLight1; // 0x0
    // public const string PixelLight2; // 0x0
    // public const string PLight1_Pos; // 0x0
    // public const string PLight2_Pos; // 0x0
    // public const string PLight1_Col; // 0x0
    // public const string PLight2_Col; // 0x0
    public bool UsePixelLights; // 0x10
    public List<string> PixelLights; // 0x18

	public void SetLight1() { }
    public void SetLight2() { }
    public void Clear() { }
}