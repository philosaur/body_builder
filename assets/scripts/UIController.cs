using Godot;
using System;

public partial class UIController : Control
{
    [Export] public NodePath BodyPath;
    private Body _body;
    private HSlider _segmentsSlider;
    private Label _segmentsValueLabel;
    private HSlider _crossSectionsSlider;
    private Label _crossSectionsValueLabel;

    public override void _Ready()
    {
        _body = GetNode<Body>(BodyPath);
        
        // Set up segments slider
        _segmentsSlider = GetNode<HSlider>("ControlPanel/VBoxContainer/HSlider");
        _segmentsValueLabel = GetNode<Label>("ControlPanel/VBoxContainer/ValueLabel");
        _segmentsSlider.ValueChanged += OnSegmentsChanged;
        _segmentsSlider.Value = _body.CrossSectionPointCount;
        _segmentsValueLabel.Text = _body.CrossSectionPointCount.ToString();

        // Set up cross sections slider
        _crossSectionsSlider = GetNode<HSlider>("ControlPanel/VBoxContainer/HSlider2");
        _crossSectionsValueLabel = GetNode<Label>("ControlPanel/VBoxContainer/ValueLabel2");
        _crossSectionsSlider.ValueChanged += OnCrossSectionsChanged;
        _crossSectionsSlider.Value = _body.CrossSectionCount;
        _crossSectionsValueLabel.Text = _body.CrossSectionCount.ToString();
    }

    private void OnSegmentsChanged(double value)
    {
        int segments = (int)value;
        _body.CrossSectionPointCount = segments;
        _segmentsValueLabel.Text = segments.ToString();
        _body.RegenerateGeometry();
    }

    private void OnCrossSectionsChanged(double value)
    {
        int crossSections = (int)value;
        _body.CrossSectionCount = crossSections;
        _crossSectionsValueLabel.Text = crossSections.ToString();
        _body.RegenerateGeometry();
    }
} 