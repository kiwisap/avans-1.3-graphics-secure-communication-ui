using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Assets.Code.Models;

public class LoginManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login Inputs")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;

    [Header("Register Inputs")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;

    [Header("Feedback")]
    public TMP_Text feedbackText;

    private AuthService authService;

    private void Start()
    {
        ShowLogin();
        if (feedbackText != null) feedbackText.text = "";
        
        // Find or create AuthService
        authService = FindAnyObjectByType<AuthService>();
        if (authService == null)
        {
            GameObject authGO = new GameObject("AuthService");
            authService = authGO.AddComponent<AuthService>();
        }
    }

    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";
    }

    public void ShowRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        if (feedbackText != null) feedbackText.text = "";
    }

    public async void OnLoginClicked()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Vul e-mailadres en wachtwoord in.");
            return;
        }

        ShowFeedback($"Inloggen als {username}...");
        ApiResult result = await authService.LoginAsync(username, password);
        
        if (result.Ok)
        {
            ShowFeedback("Inloggen gelukt!");

            // Load environment selector scene
            await SceneManager.LoadSceneAsync("EnvironmentSelectorScene", LoadSceneMode.Single);
        }
        else
        {
            ShowFeedback("Inloggen niet gelukt. Controleer gegevens.");
            Debug.LogError($"{result.Error}");
        }
    }

    public async void OnRegisterClicked()
    {
        string username = registerUsernameInput.text;
        string password = registerPasswordInput.text;
        string confirmPassword = registerConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowFeedback("Vul alle velden in.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowFeedback("Wachtwoorden komen niet overeen.");
            return;
        }

        ShowFeedback($"Gebruiker {username} registreren...");
        ApiResult result = await authService.RegisterAsync(username, password);

        if (result.Ok)
        {
            ShowFeedback("Registratie gelukt! Log in");
            ShowLogin();
        }
        else
        {
            ShowFeedback("Registratie niet gelukt.");
            Debug.LogError($"{result.Error}");
        }
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
        Debug.Log(message);
    }
}
