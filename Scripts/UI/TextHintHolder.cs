using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextHintHolder : ToolTip
{
    private void Start()
    {
        AudioManager.instance.PlayWorldSound(_audioSource, "tooltipText");
    }
}
