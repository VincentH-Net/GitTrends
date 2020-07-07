using Bounds = System.Linq.Expressions.Expression<System.Func<Xamarin.Forms.Rectangle>>;
using Expression = System.Linq.Expressions.Expression<System.Func<double>>;
using ParentMeasure = System.Func<Xamarin.Forms.RelativeLayout, double>;
using ViewMeasure = System.Func<Xamarin.Forms.RelativeLayout, Xamarin.Forms.View, double>;
using static Xamarin.Forms.Constraint;

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
        public static UnconstrainedView Unconstrained<TView>(this TView view) where TView : View => new UnconstrainedView(view);

        public static ConstraintsConstrainedView Constrain<TView>(this TView view) where TView : View => new ConstraintsConstrainedView(view);

        public static BoundsConstrainedView Constrain<TView>(this TView view, Bounds bounds) where TView : View => new BoundsConstrainedView(view, bounds);
    }

    public class UnconstrainedView : ConstrainedView
    {
        public UnconstrainedView(View view) : base(view) { }

        public override void AddTo(RelativeLayout layout) => ((Layout<View>)layout).Children.Add(view);
    }

    public class BoundsConstrainedView : ConstrainedView
    {
        readonly Bounds bounds;

        public BoundsConstrainedView(View view, Bounds bounds) : base(view) { this.bounds = bounds; }

        public override void AddTo(RelativeLayout layout) => layout.Children.Add(view, bounds);
    }

    public class ExpressionsConstrainedView : ConstrainedView
    {
        Expression? x, y, width, height;

        public ExpressionsConstrainedView(View view) : base(view) { }

        public ConstrainedView X     (Expression x     ) { this.x      = x;      return this; }
        public ConstrainedView Y     (Expression y     ) { this.y      = y;      return this; }
        public ConstrainedView Width (Expression width ) { this.width  = width;  return this; }
        public ConstrainedView Height(Expression height) { this.height = height; return this; }

        public override void AddTo(RelativeLayout layout) => layout.Children.Add(view, x, y, width, height);
    }

    public class ConstraintsConstrainedView : ConstrainedView
    {
        Constraint? xConstraint, yConstraint, widthConstraint, heightConstraint;

        public ConstraintsConstrainedView(View view) : base(view) { }

        public ConstraintsConstrainedView X     (double x)      { xConstraint      = Constant(x     ); return this; }
        public ConstraintsConstrainedView Y     (double y)      { yConstraint      = Constant(y     ); return this; }
        public ConstraintsConstrainedView Width (double width)  { widthConstraint  = Constant(width ); return this; }
        public ConstraintsConstrainedView Height(double height) { heightConstraint = Constant(height); return this; }

        public ConstraintsConstrainedView X     (ParentMeasure x     ) { xConstraint      = RelativeToParent(x     ); return this; }
        public ConstraintsConstrainedView Y     (ParentMeasure y     ) { yConstraint      = RelativeToParent(y     ); return this; }
        public ConstraintsConstrainedView Width (ParentMeasure width ) { widthConstraint  = RelativeToParent(width ); return this; }
        public ConstraintsConstrainedView Height(ParentMeasure height) { heightConstraint = RelativeToParent(height); return this; }
               
        public ConstraintsConstrainedView X     (View view, ViewMeasure x     ) { xConstraint      = RelativeToView(view, x     ); return this; }
        public ConstraintsConstrainedView Y     (View view, ViewMeasure y     ) { yConstraint      = RelativeToView(view, y     ); return this; }
        public ConstraintsConstrainedView Width (View view, ViewMeasure width ) { widthConstraint  = RelativeToView(view, width ); return this; }
        public ConstraintsConstrainedView Height(View view, ViewMeasure height) { heightConstraint = RelativeToView(view, height); return this; }

        public override void AddTo(RelativeLayout layout) => layout.Children.Add(view, xConstraint, yConstraint, widthConstraint, heightConstraint);
    }

    public abstract class ConstrainedView
    {
        readonly protected View view;

        protected ConstrainedView(View view) => this.view = view;

        public abstract void AddTo(RelativeLayout layout);
    }
}