using UnityEngine;

public class RuneDisplay : MonoBehaviour
{
    public enum SkillType { Sprint, GroundPound, Jump }
    public enum SkillState { Désactivé, Activé, Niveau2, Niveau3 }

    [Header("Paramètres")]
    public SkillType competence;
    public SkillState etat;

    [Header("GameObjects des États")]
    public GameObject etatDésactivé;
    public GameObject etatActivé;
    public GameObject etatNiveau2;
    public GameObject etatNiveau3;

    [Header("GameObjects des Compétences")]
    public GameObject compSprint;
    public GameObject compGroundPound;
    public GameObject compJump;

    /// <summary>
    /// Définit l'état du rune et met à jour visuellement immédiatement.
    /// </summary>
    public void SetState(SkillState newState)
    {
        etat = newState;
        ToggleEtat();
        ToggleCompetence();
    }

    private void ToggleEtat()
    {
        if (etatDésactivé) etatDésactivé.SetActive(etat == SkillState.Désactivé);
        if (etatActivé)    etatActivé.SetActive(etat == SkillState.Activé);
        if (etatNiveau2)   etatNiveau2.SetActive(etat == SkillState.Niveau2);
        if (etatNiveau3)   etatNiveau3.SetActive(etat == SkillState.Niveau3);
    }

    private void ToggleCompetence()
    {
        // Si état désactivé, désactiver tous les GO de compétence
        if (etat == SkillState.Désactivé)
        {
            if (compSprint) compSprint.SetActive(false);
            if (compGroundPound) compGroundPound.SetActive(false);
            if (compJump) compJump.SetActive(false);
            return;
        }

        // Sinon, activer celui qui correspond
        if (compSprint)       compSprint.SetActive(competence == SkillType.Sprint);
        if (compGroundPound)  compGroundPound.SetActive(competence == SkillType.GroundPound);
        if (compJump)         compJump.SetActive(competence == SkillType.Jump);
    }
}
