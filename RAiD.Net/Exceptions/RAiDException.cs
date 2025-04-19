// This program has been developed by students from the bachelor Computer Science at Utrecht
// University within the Software Project course.
// 
// © Copyright Utrecht University (Department of Information and Computing Sciences)

namespace RAiD.Net.Exceptions;

public class RAiDException : Exception
{
    public RAiDException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
