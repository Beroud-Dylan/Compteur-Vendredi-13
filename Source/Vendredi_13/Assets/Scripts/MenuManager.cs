using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameObject popUpPanel;
    private Animator popUpAnimator;
    [SerializeField] private InputField birthDate;
    [SerializeField] private Text nbVendrediText;

    [Header("Error Texts")]
    [SerializeField] GameObject errorTextPrefab;
    [SerializeField] Transform errorTextsParent;
    [SerializeField] int sizeOfPool;
    [SerializeField] float fadingTime;
    [SerializeField] float fadingUpSpeed;
    private Text[] errorTexts;

    void Start()
    {
        // On initialise les composants
        popUpPanel.SetActive(false);
        popUpAnimator = popUpPanel.GetComponent<Animator>();

        // On initialise la pool des textes d'erreurs
        InitializeErrorTextsPool();
    }

    #region Fermeture du Pop-Up
    public void ClosePopUp()
    {
        birthDate.text = "";
        popUpAnimator.SetTrigger("PopUpClose");
        StartCoroutine(closePopUpCor(popUpAnimator.GetCurrentAnimatorStateInfo(0).length));
    }
    IEnumerator closePopUpCor(float duration)
    {
        // On attend le temps de l'animation
        yield return new WaitForSeconds(duration);
        // On ferme le pop-up
        popUpPanel.SetActive(false);
    }
    #endregion

    #region Ouverture du Pop-Up
    public void OpenPopUp()
    {

        // On récupère la "date de naissance"
        string inputDate = birthDate.text;
        // On vérifie qu'on a bien eu un texte d'écrit
        if (string.IsNullOrEmpty(inputDate))
        {
            StartCoroutine(ErrorTextAnimation(Input.mousePosition, "Veuillez donner votre date de naissance."));
            return;
        }
        // On tente de récupérer la chaîne de caractère sous forme de date
        DateTime dateNaissance;
        if (!DateTime.TryParse(inputDate, out dateNaissance))
        {
            // On précise à l'utilisateur qu'il n'a pas donné un format de date de naissance valide
            StartCoroutine(ErrorTextAnimation(Input.mousePosition, "Le format de la date de naissance est invalide. Écrivez votre date de naissance sous le format DD/MM/YYYY."));
            return;
        }

        // On récupère la date actuelle
        DateTime dateActuelle = DateTime.Now;
        // On initialise le compteur de vendredi 13
        int nombreVendredi13 = 0;

        // On boucle à travers toutes les dates entre la date de naissance et la date actuelle
        for (DateTime date = dateNaissance; date <= dateActuelle; date = date.AddDays(1))
        {
            // On vérifie si la date est un vendredi 13
            if (date.Day == 13 && date.DayOfWeek == DayOfWeek.Friday)
            {
                // On incrément le compteur dans ce cas
                nombreVendredi13++;
            }
        }

        // On modify le texte du pop-up
        nbVendrediText.text = nombreVendredi13.ToString();

        // On affiche le pop-up
        popUpPanel.SetActive(true);
    }
    #endregion

    #region Gestion des Erreurs
    private void InitializeErrorTextsPool()
    {
        // Initialize the array size
        errorTexts = new Text[sizeOfPool];

        for (int i = 0; i < sizeOfPool; i++)
        {
            // Create it
            GameObject currentText = Instantiate(errorTextPrefab, errorTextsParent);
            currentText.transform.localPosition = Vector3.zero;
            // Assign it to the array
            errorTexts[i] = currentText.GetComponent<Text>();
            // Disable it
            errorTexts[i].enabled = false;
        }
    }

    IEnumerator ErrorTextAnimation(Vector3 position, string message)
    {
        // Get an error text
        Text ErrorText = GetAvailableErrorText();

        // Enable the error text with the right message and place it at the right position
        ErrorText.text = message;
        ErrorText.transform.position = position;
        ErrorText.enabled = true;

        // Get the time to wait
        WaitForSeconds timeToWait = new WaitForSeconds(Time.fixedDeltaTime);

        // Loop during the fading process
        for (float count = 0f; count < fadingTime; count += Time.fixedDeltaTime)
        {
            // Make the error text fade up
            ErrorText.transform.position += Vector3.up * fadingUpSpeed * Time.fixedDeltaTime;

            // Update the alpha
            ErrorText.color = new Color(ErrorText.color.r, ErrorText.color.g, ErrorText.color.b, 1f - (count / fadingTime));

            // Wait the needed amount of time
            yield return timeToWait;
        }

        // Reset the alpha correctly
        ErrorText.color = new Color(ErrorText.color.r, ErrorText.color.g, ErrorText.color.b, 1f);

        // Disable the error text
        ErrorText.enabled = false;
    }

    private Text GetAvailableErrorText()
    {
        // For every error texts
        for (int i = 0; i < sizeOfPool; i++)
        {
            // If the current one isn't enabled, it isn't used, so return it
            if (!errorTexts[i].enabled)
            {
                return errorTexts[i];
            }
        }

        // If we reach this code, then there wasn't enough error texts to cover every problem, so signal the developper and return a default value
        #if UNITY_EDITOR
            Debug.LogError("THERE WEREN'T ENOUGH TEXT ERROR, PLEASE INCREASE THE SIZE OF THE POOL !");
        #endif
        return errorTexts[0];
    }
    #endregion
}
