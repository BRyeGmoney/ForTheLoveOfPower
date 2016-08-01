using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class MAComponentPatch {
    private Dictionary<string, object> _values;

    public MAComponentPatch(Component component) {
        ComponentObject = component;
    }
    private bool _isComponentObjectNull = true;
    private Component _componentObject;
    private Component ComponentObject {
        get {
            return _componentObject;
        }
        set {
            _componentObject = value;
            _isComponentObjectNull = _componentObject == null;
        }
    }

    public string ComponentName {
        get {
            var parts = ComponentObject.GetType().ToString().Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            return parts[parts.Length - 1];
        }
    }

    public void StoreSettings() {
        if (ComponentObject == null) {
            return;
        }

        _values = new Dictionary<string, object>();

        var properties = GetProperties();
        var fields = GetFields();

        foreach (var property in properties) {
            _values.Add(property.Name, property.GetValue(ComponentObject, null));
        }
        foreach (var field in fields) {
            _values.Add(field.Name, field.GetValue(ComponentObject));
        }
    }

    private IEnumerable<FieldInfo> GetFields() {
        var fields = new List<FieldInfo>();

        foreach (var fieldInfo in ComponentObject.GetType().GetFields()) {
            if (!fieldInfo.IsPublic) {
                continue;
            }

            if (!Attribute.IsDefined(fieldInfo, typeof(HideInInspector))) {
                fields.Add(fieldInfo);
            }
        }

        return fields;
    }

    private IEnumerable<PropertyInfo> GetProperties() {
        var properties = new List<PropertyInfo>();

        foreach (var propertyInfo in ComponentObject.GetType().GetProperties()) {
            if (Attribute.IsDefined(propertyInfo, typeof (HideInInspector)))
            {
                continue;
            }

            var setMethod = propertyInfo.GetSetMethod();
            if (null != setMethod && setMethod.IsPublic) {
                properties.Add(propertyInfo);
            }
        }

        return properties;
    }

    //return component is changes have been made
    public Component RestoreSettings() {
        Component resultChangedComponent = null;

        if (!_isComponentObjectNull) {
            ComponentObject = EditorUtility.InstanceIDToObject(ComponentObject.GetInstanceID()) as Component;
        } else {
            ComponentObject = null;
        }

        if (ComponentObject != null && _values != null) {
            foreach (var name in _values.Keys) {
                var newValue = _values[name];

                var property = ComponentObject.GetType().GetProperty(name);

                if (null != property) {
                    var currentValue = property.GetValue(ComponentObject, null);

                    if (!HasValueChanged(newValue, currentValue))
                    {
                        continue;
                    }
                    property.SetValue(ComponentObject, newValue, null);
                    resultChangedComponent = ComponentObject;
                } else {
                    var field = ComponentObject.GetType().GetField(name);
                    var currentValue = field.GetValue(ComponentObject);

                    if (!HasValueChanged(newValue, currentValue))
                    {
                        continue;
                    }

                    field.SetValue(ComponentObject, newValue);
                    resultChangedComponent = ComponentObject;
                }
            }
        }

        _values = null;

        return resultChangedComponent;
    }

    private static bool HasValueChanged(object newValue, object oldValue) {
        var valuesChanged = true;

        if (null != newValue && null != oldValue) {
            var valueToCompare = newValue as IComparable;

            if (null == valueToCompare) {
                try {
                    var serializer = new XmlSerializer(newValue.GetType());

                    using (var streamNew = new MemoryStream()) {
                        serializer.Serialize(streamNew, newValue);

                        var encoding = new UTF8Encoding();

                        var oldValueSerialized = encoding.GetString(streamNew.ToArray());

                        using (var streamOld = new MemoryStream()) {
                            serializer.Serialize(streamOld, oldValue);

                            var newValueSerialized = encoding.GetString(streamOld.ToArray());

                            valuesChanged = !string.Equals(newValueSerialized, oldValueSerialized);
                        }
                    }
                }
                catch {
                    valuesChanged = true;
                }
            } else {
                valuesChanged = valueToCompare.CompareTo(oldValue) != 0;
            }
        } else if (null == oldValue && null == newValue) {
            valuesChanged = false;
        }

        return valuesChanged;
    }
}
