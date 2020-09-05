﻿using UnityEngine;
using UnityEngine.UI;


static class UICreatorInputField
{
    //TODO return rectTarnsform
    public static void Create(RectTransform _parent, string _name, UIAnchor _anchor, Vector2 _anchoredPosition, Vector2 _pivot, Vector2 _SizeDelta, string _styleName)
    {
        UIFactory.UIInputField(
         UIFactory.UIRectTransform(
             _parent,
             _name,
             _anchor,
             _anchoredPosition,
             _pivot,
             _SizeDelta),
         _styleName);
    }
}
