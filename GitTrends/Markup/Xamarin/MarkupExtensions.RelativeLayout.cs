using Bounds = System.Linq.Expressions.Expression<System.Func<Xamarin.Forms.Rectangle>>;
using Expression = System.Linq.Expressions.Expression<System.Func<double>>;

namespace Xamarin.Forms.Markup
{
    public static partial class Factories
    {
        public static RelativeLayout RelativeLayout(params ConstrainedView?[] constrainedViews)
        {
            var layout = new RelativeLayout();
            foreach (var constrainedView in constrainedViews) constrainedView?.AddTo(layout);
            return layout;
        }
    }

    public static class ConstrainViewExtensions
    {
        public static ConstrainedView UnConstrained<TView>(this TView view) where TView : View
        => ConstrainedView.FromView(view);

        public static ConstrainedView Constrain<TView>(this TView view, Bounds bounds) where TView : View
        => ConstrainedView.FromBounds(view, bounds);

        public static ConstrainedView Constrain<TView>(this TView view, Expression? x = null, Expression? y = null, Expression? width = null, Expression? height = null) where TView : View
        => ConstrainedView.FromExpressions(view, x, y, width, height);

        public static ConstrainedView Constrain<TView>(this TView view, Constraint? xConstraint = null, Constraint? yConstraint = null, Constraint? widthConstraint = null, Constraint? heightConstraint = null) where TView : View
        => ConstrainedView.FromConstraints(view, xConstraint, yConstraint, widthConstraint, heightConstraint);
    }

    public class ConstrainedView
    {
        enum Kind { None, Bounds, Expressions, Constraints }

        readonly View view;
        Kind kind;
        Bounds? bounds;
        Expression? x, y, width, height;
        Constraint? xConstraint, yConstraint, widthConstraint, heightConstraint;

        ConstrainedView(View view) => this.view = view;

        internal void AddTo(RelativeLayout layout)
        {
            switch (kind)
            {
                case Kind.None: ((Layout<View>)layout).Children.Add(view); break;
                case Kind.Bounds: layout.Children.Add(view, bounds); break;
                case Kind.Expressions: layout.Children.Add(view, x, y, width, height); break;
                case Kind.Constraints: layout.Children.Add(view, xConstraint, yConstraint, widthConstraint, heightConstraint); break;
            }
        }

        internal static ConstrainedView FromView(View view)
            => new ConstrainedView(view) { kind = Kind.None };

        internal static ConstrainedView FromBounds(View view, Bounds bounds)
            => new ConstrainedView(view) { kind = Kind.Bounds, bounds = bounds };

        internal static ConstrainedView FromExpressions(View view, Expression? x, Expression? y, Expression? width, Expression? height)
            => new ConstrainedView(view) { kind = Kind.Expressions, x = x, y = y, width = width, height = height };

        internal static ConstrainedView FromConstraints(View view, Constraint? xConstraint, Constraint? yConstraint, Constraint? widthConstraint, Constraint? heightConstraint)
            => new ConstrainedView(view) { kind = Kind.Constraints, xConstraint = xConstraint, yConstraint = yConstraint, widthConstraint = widthConstraint, heightConstraint = heightConstraint };
    }
}