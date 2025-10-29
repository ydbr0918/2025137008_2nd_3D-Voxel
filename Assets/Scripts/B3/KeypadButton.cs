using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    public KeypadLock target;   // 같은 패널에 있는 KeypadLock
    public enum Type { Digit, Backspace, Clear, Submit }
    public Type type = Type.Digit;
    public int digit = 0;       // 0~9

    public void Press()
    {
        if (!target) return;

        switch (type)
        {
            case Type.Digit: target.PressDigit(digit); break;
            case Type.Backspace: target.Backspace(); break;
            case Type.Clear: target.ClearAll(); break;
            case Type.Submit: target.Submit(); break;
        }
    }
}
