using System;
using System.Linq;
using KSP.UI;
using KSP.UI.Screens;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace Hire
{
    // We'll have to wait until the AstronautComplex UI is actually spawned in
    // order to grab the dimensions of the area we'll be replacing. All the
    // visual bits should be handled by now so as soon as the panel is actually
    // visible, we can grab those dimensions and create the custom GUI using them
    class CustomAstronautComplexUISpawner : MonoBehaviour
    {
       
        private void Start()
        {
#if DEBUG
           DebugDimensions();
#endif
            var vectors = new Vector3[4];
            var uiCam = UIMasterController.Instance.GetComponentInChildren<UIMainCamera>();

            if (uiCam == null)
            {
                Hire.Log.Error("UIMainCamera not found");
                return;
            }

            var camera = uiCam.GetComponent<Camera>();

            if (camera == null)
            {
                Hire.Log.Error("Camera attached to UIMainCamera not found");
                return;
            }

            GetComponent<RectTransform>().GetWorldCorners(vectors);

            for (int i = 0; i < 4; ++i)
                vectors[i] = camera.WorldToScreenPoint(vectors[i]);


            // note: these are in screen space
            var rect = new Rect(vectors[1].x, Screen.height - vectors[1].y, vectors[2].x - vectors[1].x,
                vectors[2].y - vectors[3].y);

            gameObject.AddComponent<CustomAstronautComplexUI>().Initialize(rect);

            Destroy(this);
        }


        private void DebugDimensions()
        {
            Hire.Log.Info("Debugging dimensions");
            var vectors = new Vector3[4];
            var camera = UIMasterController.Instance.GetComponentInChildren<UIMainCamera>().GetComponent<Camera>();

            GetComponent<RectTransform>().GetWorldCorners(vectors);

            foreach (var item in vectors)
                Hire.Log.Info("Corner: " + item);

            for (int i = 0; i < 4; ++i)
            {
                vectors[i] = camera.GetComponent<Camera>().WorldToScreenPoint(vectors[i]);
                Hire.Log.Info("Transformed corner: " + vectors[i]);
            }
        }
    }

}
