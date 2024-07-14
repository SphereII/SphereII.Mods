/// <summary>
/// Interface for dialog actions or requirements that accept an "operator" attribute.
/// It places no restrictions on the values that the "operator" attribute may have.
/// </summary>
public interface IDialogOperator
{
    /// <summary>
    /// Contains the raw string value of the "operator" attribute.
    /// </summary>
    string Operator { get; set; }
}
