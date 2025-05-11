using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class S_ParticuleTourbillonJump : MonoBehaviour
{
    [Header("Référence au PS enfant (Tourbillon)")]
    public ParticleSystem childTourbillonPs;

    [Header("Taille 3D de départ du Tourbillon (X, Y, Z)")]
    public Vector3 childStartSize3D = new Vector3(275f, 275f, 80f);

    private ParticleSystem.MainModule parentMain;
    private ParticleSystem.MainModule childMain;

    void OnValidate() => ApplyToChild();
    void Awake() => ApplyToChild();

    void ApplyToChild()
    {
        if (childTourbillonPs == null)
            return;

        // 0) On stoppe le PS enfant pour pouvoir modifier la duration
        childTourbillonPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 1) Récupère les MainModules
        parentMain = GetComponent<ParticleSystem>().main;
        childMain = childTourbillonPs.main;

        // 2) Hérite Loop & PlayOnAwake
        childMain.loop = parentMain.loop;
        childMain.playOnAwake = parentMain.playOnAwake;

        // 3) Synchronise Duration & StartLifetime
        childMain.duration = parentMain.duration;
        childMain.startLifetime = parentMain.startLifetime;

        // 4) Active le 3D Start Size et applique X/Y/Z
        childMain.startSize3D = true;
        childMain.startSizeX = new ParticleSystem.MinMaxCurve(childStartSize3D.x);
        childMain.startSizeY = new ParticleSystem.MinMaxCurve(childStartSize3D.y);
        childMain.startSizeZ = new ParticleSystem.MinMaxCurve(childStartSize3D.z);

        // 5) Relance si nécessaire
        if (childMain.playOnAwake)
            childTourbillonPs.Play();
    }
}