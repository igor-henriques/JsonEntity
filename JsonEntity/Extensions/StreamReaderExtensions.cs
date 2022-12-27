namespace JsonEntity.Extensions;

internal static class StreamReaderExtensions
{
    internal static string ReadLineBackwards(this StreamReader reader)
    {
        const string endReaderStringCheck = "{\"Id\":";
        long streamReaderPosition = reader.BaseStream.Position;
        reader.BaseStream.Seek(0, SeekOrigin.End);

        // StringBuilder para armazenar a linha lida
        StringBuilder line = new();

        // Começa a ler a partir do último caractere da Stream        
        while (
            !line.ToString().Contains(endReaderStringCheck, StringComparison.CurrentCultureIgnoreCase) | 
            streamReaderPosition is 0)
        {
            var character = (char)reader.Read();            
            line.Append(character);
            reader.BaseStream.Seek(streamReaderPosition, SeekOrigin.End);
            streamReaderPosition--;
        }

        return new string(line.ToString().Reverse().ToArray());
    }
}
