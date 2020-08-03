using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallofGenerator {
    public static float[, ] GenerateFallofMap (int size) {
        float[, ] map = new float[size, size];
        // float f = (float) ;
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float x = (i / (float) size) * 2 - 1;
                float y = (j / (float) size) * 2 - 1;

                map[i, j] = Mathf.Max (Mathf.Abs (x), Mathf.Abs (y));
                if (i == j) Debug.Log (i + "," + j + ":" + x + "," + y);
            }
        }
        return map;
    }
}