using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class ManagerUI : MonoBehaviour
{
    [Header("UI - Visibilidad Menú")]
    public GameObject menuPanel;
    public KeyCode toggleKey = KeyCode.Escape;

    [Header("UI - Controles de Flocking (Boids)")]
    public Slider separationSlider;
    public TextMeshProUGUI separationText;
    public Slider alignmentSlider;
    public TextMeshProUGUI alignmentText;
    public Slider cohesionSlider;
    public TextMeshProUGUI cohesionText;

    [Header("UI - Controles del Cazador")]
    public HunterFSM hunter;
    public Slider hunterSpeedSlider;
    public TextMeshProUGUI hunterSpeedText;

    private void Start()
    {
        SetupFlockingControls();
        SetupHunterControls();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }

    #region Configuración UI Flocking (Boids)
    private void SetupFlockingControls()
    {
        float initSep = 1.5f, initAli = 1.5f, initCoh = 0.8f;
        
        // Carga los valores actuales del primer Boid activo en la escena
        if (FlockManager.Instance != null && FlockManager.Instance.Boids.Count > 0)
        {
            Boid sampleBoid = FlockManager.Instance.Boids[0];
            initSep = sampleBoid.separationWeight;
            initAli = sampleBoid.alignmentWeight;
            initCoh = sampleBoid.cohesionWeight;
        }

        if (separationSlider != null)
        {
            separationSlider.minValue = 0f;
            separationSlider.maxValue = 5f;
            separationSlider.value = initSep;
            separationSlider.onValueChanged.AddListener(OnSeparationChanged);
        }

        if (alignmentSlider != null)
        {
            alignmentSlider.minValue = 0f;
            alignmentSlider.maxValue = 5f;
            alignmentSlider.value = initAli;
            alignmentSlider.onValueChanged.AddListener(OnAlignmentChanged);
        }

        if (cohesionSlider != null)
        {
            cohesionSlider.minValue = 0f;
            cohesionSlider.maxValue = 5f;
            cohesionSlider.value = initCoh;
            cohesionSlider.onValueChanged.AddListener(OnCohesionChanged);
        }

        UpdateFlockingTexts(initSep, initAli, initCoh);
    }

    private void OnSeparationChanged(float val)
    {
        if (FlockManager.Instance == null) return;
        foreach (Boid b in FlockManager.Instance.Boids)
        {
            if (b != null) b.separationWeight = val;
        }
        UpdateFlockingTexts(val, alignmentSlider ? alignmentSlider.value : 0f, cohesionSlider ? cohesionSlider.value : 0f);
    }

    private void OnAlignmentChanged(float val)
    {
        if (FlockManager.Instance == null) return;
        foreach (Boid b in FlockManager.Instance.Boids)
        {
            if (b != null) b.alignmentWeight = val;
        }
        UpdateFlockingTexts(separationSlider ? separationSlider.value : 0f, val, cohesionSlider ? cohesionSlider.value : 0f);
    }

    private void OnCohesionChanged(float val)
    {
        if (FlockManager.Instance == null) return;
        foreach (Boid b in FlockManager.Instance.Boids)
        {
            if (b != null) b.cohesionWeight = val;
        }
        UpdateFlockingTexts(separationSlider ? separationSlider.value : 0f, alignmentSlider ? alignmentSlider.value : 0f, val);
    }

    private void UpdateFlockingTexts(float sep, float ali, float coh)
    {
        if (separationText != null) separationText.text = $"Separación: {sep:F2}";
        if (alignmentText != null) alignmentText.text = $"Alineación: {ali:F2}";
        if (cohesionText != null) cohesionText.text = $"Cohesión: {coh:F2}";
    }
    #endregion

    #region Configuración UI Cazador
    private void SetupHunterControls()
    {
        if (hunter == null) return;

        if (hunterSpeedSlider != null)
        {
            hunterSpeedSlider.minValue = 2f;
            hunterSpeedSlider.maxValue = 15f;
            hunterSpeedSlider.value = hunter.maxSpeed;
            hunterSpeedSlider.onValueChanged.AddListener(OnHunterSpeedChanged);
        }

        if (hunterSpeedText != null)
            hunterSpeedText.text = $"Velocidad Cazador: {hunter.maxSpeed:F1}";
    }

    private void OnHunterSpeedChanged(float val)
    {
        if (hunter != null)
        {
            hunter.maxSpeed = val;
            if (hunterSpeedText != null) hunterSpeedText.text = $"Velocidad Cazador: {val:F1}";
        }
    }
    #endregion
}