namespace Xamarin.Forms.Markup
{
    public static class ElementExtensions
    {
        public static TElement DynamicResource<TElement>(this TElement element, BindableProperty property, string key) where TElement : Element
        { element.SetDynamicResource(property, key); return element; }

        public static TElement DynamicResources<TElement>(this TElement element, params (BindableProperty property, string key)[] resources) where TElement : Element
        { foreach (var resource in resources) element.SetDynamicResource(resource.property, resource.key); return element; }

        public static TButton Padding<TButton>(this TButton button, double horizontalSize, double verticalSize) where TButton : Button
        { button.Padding = new Thickness(horizontalSize, verticalSize); return button; }
    }

    public static partial class Factories
    {
    }
}