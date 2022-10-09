
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace WaveFunctionCollapse2D
{
    public class WFCUIManager : MonoBehaviour
    {
        [SerializeField] private WaveFunctionCollapse2D _waveFunctionCollapse;

        [SerializeField] private Button _btnGenerate;
        [SerializeField] private Button _btnIterateManually;
        [SerializeField] private Slider _sliderIterationOffset;
        [SerializeField] private Button _btnIterateAutomatically;
        [SerializeField] private Button _btnStopIterateAutonatically;
        [SerializeField] private Button _btnGenerateAll;


        private const string PP_SIMULATION_SPEED = "Simulation_Speed";

        private void Awake()
        {
            this.DisableIfNull(_waveFunctionCollapse, "Missing WFC assignment!");
        }

        private void OnEnable()
        {
            if (_btnGenerate) _btnGenerate.onClick.AddListener(GenerateClicked);
            if (_btnIterateManually) _btnIterateManually.onClick.AddListener(IterateManuallyClicked);
            if (_sliderIterationOffset)
            {
                _sliderIterationOffset.onValueChanged.AddListener(SliderChanged);
                _sliderIterationOffset.value = PlayerPrefs.GetFloat(PP_SIMULATION_SPEED, 0.1f);
            }
            if (_btnIterateAutomatically) _btnIterateAutomatically.onClick.AddListener(StartIteration);
            if (_btnStopIterateAutonatically) _btnStopIterateAutonatically.onClick.AddListener(StopIteration);
            if (_btnGenerateAll) _btnGenerateAll.onClick.AddListener(Solve);
        }

        private void OnDisable()
        {
            if (_btnGenerate) _btnGenerate.onClick.RemoveListener(GenerateClicked);
            if (_btnIterateManually) _btnIterateManually.onClick.RemoveListener(IterateManuallyClicked);
            if (_sliderIterationOffset)
            {
                _sliderIterationOffset.onValueChanged.RemoveListener(SliderChanged);
                PlayerPrefs.SetFloat(PP_SIMULATION_SPEED, _sliderIterationOffset.value);
            }
            if (_btnIterateAutomatically) _btnIterateAutomatically.onClick.RemoveListener(StartIteration);
            if (_btnStopIterateAutonatically) _btnStopIterateAutonatically.onClick.RemoveListener(StopIteration);
            if (_btnGenerateAll) _btnGenerateAll.onClick.RemoveListener(Solve);
        }


        private void GenerateClicked() => _waveFunctionCollapse.Generate();
        private void IterateManuallyClicked() => _waveFunctionCollapse.Iterate();
        private void SliderChanged(float value) => _waveFunctionCollapse.AutoIterationDelay = value;
        private void StartIteration() => _waveFunctionCollapse.StartAutoIterate();
        private void StopIteration() => _waveFunctionCollapse.StopAutoIterate();
        private void Solve() => _waveFunctionCollapse.Solve();

    }
}
