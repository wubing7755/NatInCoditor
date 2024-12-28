using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace CoreLib;

public class Player : INotifyPropertyChanged
{
    public Player() { }

    public Player(int id, string name)
    {
        _id = id;
        Name = name;
    }

    private readonly int _id;

    private string _name;

    private object[] _contents = new object[10];

    public int Id => _id;

    public string Name
    {
        get { return _name; }
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public object[] Contents
    {
        get { return _contents; }
        set
        {
            if (value.Length == 10)
            {
                _contents = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Contents 索引器
    /// </summary>
    public object this[int index]
    {
        get
        {
            return Contents[index];
        }
        set
        {
            Contents[index] = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

public enum PlayerStatus
{
    Unknown,
    Healthy,
    UnHealthy
}

public class ImgPaths : INotifyPropertyChanged
{
    public ImgPaths()
    {
        _sources = new List<string>();
    }

    private List<string> _sources;

    private ObservableCollection<string> _sourcesCollection;

    // 获取和设置图片源列表
    public List<string> Sources
    {
        get => _sources;
        set
        {
            _sources = value;
            OnPropertyChanged(nameof(Sources));
        }
    }

    // 索引器，通过索引获取或设置单个元素
    public string this[int index]
    {
        get => Sources[index];
    }

    // 添加图片源
    public void AddSource(string source)
    {
        Sources.Add(source);
        OnPropertyChanged(nameof(Sources));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TempTest
{
    public void Run()
    {
        var nArgument = Expression.Parameter(typeof(int), "n");
        var result = Expression.Variable(typeof(int), "result");

        LabelTarget label = Expression.Label(typeof(int));

        var initializeResult = Expression.Assign(result, Expression.Constant(1));

        var block = Expression.Block(
            Expression.Assign(result, Expression.Multiply(result, nArgument)),
            Expression.PostDecrementAssign(nArgument)
            );


        //int[] array = new Array();

    }
}



public abstract class Visitor
{
    private readonly Expression node;

    protected Visitor(Expression node) => this.node = node;

    public abstract void Visit(string prefix);

    public ExpressionType NodeType => node.NodeType;

    public static Visitor CreateFromExpression(Expression node) => node.NodeType switch
    {
        ExpressionType.Constant => new ConstantVisitor((ConstantExpression)node),
        ExpressionType.Lambda => new LambdaVisitor((LambdaExpression)node),
        ExpressionType.Parameter => new ParameterVisitor((ParameterExpression)node),
        ExpressionType.Add => new BinaryVisitor((BinaryExpression)node),
        ExpressionType.Equal => new BinaryVisitor((BinaryExpression)node),
        ExpressionType.Multiply => new BinaryVisitor((BinaryExpression)node),
        ExpressionType.Conditional => new ConditionalVisitor((ConditionalExpression)node),
        ExpressionType.Call => new MethodCallVisitor((MethodCallExpression)node),
        _ => throw new NotImplementedException($"Node not processed yet: {node.NodeType}"),
    };

}


// Lambda Visitor
public class LambdaVisitor : Visitor
{
    private readonly LambdaExpression node;
    public LambdaVisitor(LambdaExpression node) : base(node) => this.node = node;

    public override void Visit(string prefix)
    {
        Console.WriteLine($"{prefix}This expression is a {NodeType} expression type");
        Console.WriteLine($"{prefix}The name of the lambda is {((node.Name == null) ? "<null>" : node.Name)}");
        Console.WriteLine($"{prefix}The return type is {node.ReturnType}");
        Console.WriteLine($"{prefix}The expression has {node.Parameters.Count} argument(s). They are:");
        // Visit each parameter:
        foreach (var argumentExpression in node.Parameters)
        {
            var argumentVisitor = CreateFromExpression(argumentExpression);
            argumentVisitor.Visit(prefix + "\t");
        }
        Console.WriteLine($"{prefix}The expression body is:");
        // Visit the body:
        var bodyVisitor = CreateFromExpression(node.Body);
        bodyVisitor.Visit(prefix + "\t");
    }
}

// Binary Expression Visitor:
public class BinaryVisitor : Visitor
{
    private readonly BinaryExpression node;
    public BinaryVisitor(BinaryExpression node) : base(node) => this.node = node;

    public override void Visit(string prefix)
    {
        Console.WriteLine($"{prefix}This binary expression is a {NodeType} expression");
        var left = CreateFromExpression(node.Left);
        Console.WriteLine($"{prefix}The Left argument is:");
        left.Visit(prefix + "\t");
        var right = CreateFromExpression(node.Right);
        Console.WriteLine($"{prefix}The Right argument is:");
        right.Visit(prefix + "\t");
    }
}

// Parameter visitor:
public class ParameterVisitor : Visitor
{
    private readonly ParameterExpression node;
    public ParameterVisitor(ParameterExpression node) : base(node)
    {
        this.node = node;
    }

    public override void Visit(string prefix)
    {
        Console.WriteLine($"{prefix}This is an {NodeType} expression type");
        Console.WriteLine($"{prefix}Type: {node.Type}, Name: {node.Name}, ByRef: {node.IsByRef}");
    }
}


internal class ConstantVisitor : Visitor
{
    private readonly ConstantExpression node;

    public ConstantVisitor(ConstantExpression node) : base(node) => this.node = node;

    public override void Visit(string prefix)
    {
        Console.WriteLine($"{prefix}This is an {NodeType} expression type");
        Console.WriteLine($"{prefix}The type of the constant value is {node.Type}");
        Console.WriteLine($"{prefix}The value of the constant value is {node.Value}");
    }
}

public class ConditionalVisitor : Visitor
{
    private readonly ConditionalExpression node;
    public ConditionalVisitor(ConditionalExpression node) : base(node)
    {
        this.node = node;
    }

    public override void Visit(string prefix)
    {
        Console.WriteLine($"{prefix}This expression is a {NodeType} expression");
        var testVisitor = Visitor.CreateFromExpression(node.Test);
        Console.WriteLine($"{prefix}The Test for this expression is:");
        testVisitor.Visit(prefix + "\t");
        var trueVisitor = Visitor.CreateFromExpression(node.IfTrue);
        Console.WriteLine($"{prefix}The True clause for this expression is:");
        trueVisitor.Visit(prefix + "\t");
        var falseVisitor = Visitor.CreateFromExpression(node.IfFalse);
        Console.WriteLine($"{prefix}The False clause for this expression is:");
        falseVisitor.Visit(prefix + "\t");
    }
}

public class MethodCallVisitor : Visitor
{
    private readonly MethodCallExpression node;
    public MethodCallVisitor(MethodCallExpression node) : base(node)
    {
        this.node = node;
    }

    public override void Visit(string prefix)
    {
        Console.WriteLine($"{prefix}This expression is a {NodeType} expression");
        if (node.Object == null)
            Console.WriteLine($"{prefix}This is a static method call");
        else
        {
            Console.WriteLine($"{prefix}The receiver (this) is:");
            var receiverVisitor = Visitor.CreateFromExpression(node.Object);
            receiverVisitor.Visit(prefix + "\t");
        }

        var methodInfo = node.Method;
        Console.WriteLine($"{prefix}The method name is {methodInfo.DeclaringType}.{methodInfo.Name}");
        // There is more here, like generic arguments, and so on.
        Console.WriteLine($"{prefix}The Arguments are:");
        foreach (var arg in node.Arguments)
        {
            var argVisitor = Visitor.CreateFromExpression(arg);
            argVisitor.Visit(prefix + "\t");
        }
    }
}