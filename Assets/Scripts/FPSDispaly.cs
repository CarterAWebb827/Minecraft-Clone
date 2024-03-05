using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSDispaly : MonoBehaviour {
    private float fps;
    public TMPro.TextMeshProUGUI fpsText;

    private void Start() {
        InvokeRepeating("GetFPS", 1, 1);
    }

    private void GetFPS() {
        fps = (int)(1f / Time.unscaledDeltaTime);
        fpsText.text = "FPS: " + fps.ToString();
    }
}
