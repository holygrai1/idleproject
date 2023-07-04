using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleViewRestarter : ViewRestarter
{
    public override void Restart(ViewNormal view)
    {
        var parts = view.GetComponentsInChildren<ParticleSystem>();

        foreach (var part in parts)
        {
            part.Stop();
            part.Clear();
            part.Play();
        }
    }
}

