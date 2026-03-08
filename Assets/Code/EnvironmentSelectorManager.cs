using Assets.Code.Models;
using Assets.Code.Services;
using Assets.Code.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Code
{
    public class EnvironmentSelectorManager : MonoBehaviour
    {
        [Header("UI Root")]
        public GameObject rootPanel;

        [Header("List")]
        public RectTransform environmentListContent;
        public EnvironmentListItemView environmentItemTemplate;

        [Header("Top Buttons")]
        public Button backToLoginButton;
        public Button openCreateEnvironmentModalButton;

        [Header("Modals")]
        public ConfirmModalView confirmDeleteModal;
        public CreateEnvironmentModalView createEnvironmentModal;

        [Header("Feedback")]
        public TMP_Text feedbackText;

        private Environment2DService service;

        private readonly List<Environment2DDto> _environments = new();
        private int? _pendingDeleteEnvironmentId;

        private void Awake()
        {
            if (service == null)
            {
                var apiGO = new GameObject("Environment2DService");
                apiGO.transform.SetParent(transform, false);
                service = apiGO.AddComponent<Environment2DService>();
            }
        }

        private void Start()
        {
            HookUI();
            RunTask(RefreshEnvironmentsAsync);
        }

        private void HookUI()
        {
            backToLoginButton.onClick.RemoveAllListeners();
            backToLoginButton.onClick.AddListener(BackToLogin);

            openCreateEnvironmentModalButton.onClick.RemoveAllListeners();
            openCreateEnvironmentModalButton.onClick.AddListener(ShowCreateEnvironmentModal);

            confirmDeleteModal.cancelButton.onClick.RemoveAllListeners();
            confirmDeleteModal.cancelButton.onClick.AddListener(confirmDeleteModal.Hide);

            confirmDeleteModal.confirmButton.onClick.RemoveAllListeners();
            confirmDeleteModal.confirmButton.onClick.AddListener(ConfirmDeleteClicked);

            createEnvironmentModal.cancelButton.onClick.RemoveAllListeners();
            createEnvironmentModal.cancelButton.onClick.AddListener(createEnvironmentModal.Hide);

            createEnvironmentModal.createButton.onClick.RemoveAllListeners();
            createEnvironmentModal.createButton.onClick.AddListener(CreateEnvironmentClicked);
        }

        public void BackToLogin()
        {
            feedbackText.text = "Terug naar inloggen";

            // Unload current scene and load login scene
            SceneManager.LoadScene("AuthScene", LoadSceneMode.Single);
        }

        public void ShowCreateEnvironmentModal()
        {
            feedbackText.text = "";
            createEnvironmentModal.Show();
        }

        // --- UI Event wrappers (void) ---
        private void ConfirmDeleteClicked() => RunTask(OnConfirmDeleteAsync);
        private void CreateEnvironmentClicked() => RunTask(OnCreateEnvironmentAsync);

        // --- Async logic ---
        private async Task RefreshEnvironmentsAsync()
        {
            feedbackText.text = "Environments laden...";

            var res = await service.GetEnvironmentsAsync();
            if (!res.Ok)
            {
                feedbackText.text = res.Error;
                return;
            }

            _environments.Clear();
            _environments.AddRange(res.Value);

            BuildListUI();
            feedbackText.text = _environments.Count == 0 ? "Geen environments gevonden." : "";
        }

        private void BuildListUI()
        {
            for (int i = environmentListContent.childCount - 1; i >= 0; i--)
                Destroy(environmentListContent.GetChild(i).gameObject);

            foreach (var env in _environments)
            {
                var item = Instantiate(environmentItemTemplate, environmentListContent);
                item.gameObject.SetActive(true);
                item.gameObject.name = $"EnvironmentItem_{env.Id}";

                item.environmentNameText.text = $"{env.Name} ({env.MaxLength}x{env.MaxHeight})";

                item.openButton.onClick.RemoveAllListeners();
                item.openButton.onClick.AddListener(() => OpenEnvironment(env));

                item.deleteButton.onClick.RemoveAllListeners();
                item.deleteButton.onClick.AddListener(() => AskDeleteEnvironment(env));
            }
        }

        private async void OpenEnvironment(Environment2DDto env)
        {
            PlayerPrefs.SetInt("Environment_Id", env.Id);
            PlayerPrefs.SetString("Environment_Name", env.Name);
            PlayerPrefs.SetInt("Environment_MaxHeight", env.MaxHeight);
            PlayerPrefs.SetInt("Environment_MaxLength", env.MaxLength);

            await SceneManager.LoadSceneAsync("EnvironmentEditorScene", LoadSceneMode.Single);
        }

        private void AskDeleteEnvironment(Environment2DDto env)
        {
            _pendingDeleteEnvironmentId = env.Id;
            confirmDeleteModal.bodyText.text = $"Weet je zeker dat je '{env.Name}' wilt verwijderen?";
            confirmDeleteModal.Show();
        }

        private async Task OnConfirmDeleteAsync()
        {
            confirmDeleteModal.Hide();

            if (!_pendingDeleteEnvironmentId.HasValue)
                return;

            int id = _pendingDeleteEnvironmentId.Value;
            _pendingDeleteEnvironmentId = null;

            feedbackText.text = "Verwijderen...";

            var res = await service.DeleteEnvironmentAsync(id);
            if (!res.Ok)
            {
                feedbackText.text = res.Error;
                return;
            }

            feedbackText.text = "Environment verwijderd.";
            await RefreshEnvironmentsAsync();
        }

        private async Task OnCreateEnvironmentAsync()
        {
            string name = (createEnvironmentModal.nameInput.text ?? "").Trim();

            bool okW = int.TryParse(createEnvironmentModal.widthInput.text, out int width);
            bool okH = int.TryParse(createEnvironmentModal.heightInput.text, out int height);

            if (name.Length < 1) { feedbackText.text = "Naam is verplicht."; return; }
            if (!okW || width < 20 || width > 100) { feedbackText.text = "Breedte moet 20 t/m 100 zijn."; return; }
            if (!okH || height < 10 || height > 100) { feedbackText.text = "Hoogte moet 10 t/m 100 zijn."; return; }

            createEnvironmentModal.Hide();
            feedbackText.text = "Environment aanmaken...";

            var req = new Environment2DDto { Name = name, MaxLength = width, MaxHeight = height };

            var res = await service.CreateEnvironmentAsync(req);
            if (!res.Ok)
            {
                feedbackText.text = res.Error;
                return;
            }

            feedbackText.text = "Environment aangemaakt.";
            await RefreshEnvironmentsAsync();
        }

        private void RunTask(Func<Task> taskFactory)
        {
            _ = RunTaskInternal(taskFactory);
        }

        private async Task RunTaskInternal(Func<Task> taskFactory)
        {
            try
            {
                await taskFactory();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (feedbackText != null)
                    feedbackText.text = "Onverwachte fout: " + e.Message;
            }
        }
    }
}