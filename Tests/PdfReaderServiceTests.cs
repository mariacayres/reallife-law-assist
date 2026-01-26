using System;
using System.IO;
using System.Linq;
using Xunit;
using RealLifeLawAssist.Services;

namespace RealLifeLawAssist.Tests
{
    /// <summary>
    /// Classe de testes para validar o comportamento do PdfReaderService.
    /// Utiliza xUnit para definir os casos de teste.
    /// </summary>
    public class PdfReaderServiceTests
    {
        private readonly PdfReaderService _pdfService;
        private readonly string _testPdfFolder;

        /// <summary>
        /// Construtor da classe de teste.
        /// Inicializa o serviço que será testado antes de cada teste.
        /// </summary>
        public PdfReaderServiceTests()
        {
            _pdfService = new PdfReaderService();
            // Define um caminho temporário para referência, embora o serviço use um caminho fixo internamente.
            _testPdfFolder = Path.Combine(Path.GetTempPath(), "test_pdfs");
        }

        /// <summary>
        /// Verifica se o serviço pode ser instanciado corretamente sem lançar exceções.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializeWithoutException()
        {
            // Act & Assert
            // Tenta criar uma nova instância e verifica se não é nula.
            var service = new PdfReaderService();
            Assert.NotNull(service);
        }

        /// <summary>
        /// Verifica se o método GetPdfFiles retorna uma lista não nula e não vazia
        /// quando existem arquivos na pasta configurada.
        /// </summary>
        [Fact]
        public void GetPdfFiles_WhenFolderExists_ShouldReturnPdfFiles()
        {
            // Act
            // Chama o método que lista os arquivos PDF.
            var result = _pdfService.GetPdfFiles();

            // Assert
            // Garante que a lista retornada não é nula e contém itens.
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        /// <summary>
        /// Verifica se o método ExtractTextFromPdf lança uma exceção FileNotFoundException
        /// quando o caminho do arquivo fornecido não existe.
        /// </summary>
        [Fact]
        public void ExtractTextFromPdf_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
        {
            // Arrange
            // Define um nome de arquivo que certamente não existe.
            var nonExistentPath = "non_existent_file.pdf";

            // Act & Assert
            // Verifica se a chamada do método lança a exceção esperada.
            var exception = Assert.Throws<FileNotFoundException>(() => 
                _pdfService.ExtractTextFromPdf(nonExistentPath));
            
            // Valida a mensagem de erro e o nome do arquivo na exceção.
            Assert.Equal("PDF não encontrado.", exception.Message);
            Assert.Equal(nonExistentPath, exception.FileName);
        }

        /// <summary>
        /// Verifica se o método ExtractTextFromPdf lida corretamente com entradas inválidas (nulo ou vazio),
        /// lançando a exceção apropriada.
        /// </summary>
        /// <param name="path">O caminho inválido a ser testado (string vazia ou null).</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ExtractTextFromPdf_WhenPathIsNullOrEmpty_ShouldThrowException(string path)
        {
            // Act & Assert
            // Verifica se passar null ou string vazia causa uma exceção.
            Assert.Throws<FileNotFoundException>(() => 
                _pdfService.ExtractTextFromPdf(path));
        }
    }
}