// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.PowerPlatform.PowerApps.Persistence.YamlValidator;

public class ValidationProcessor
{
    private readonly YamlLoader _fileLoader;
    private readonly SchemaLoader _schemaLoader;
    private readonly Validator _validator;

    public ValidationProcessor(YamlLoader fileLoader, SchemaLoader schemaLoader, Validator validator)
    {
        _fileLoader = fileLoader;
        _schemaLoader = schemaLoader;
        _validator = validator;
    }

    public void RunValidation(ValidationRequest inputData)
    {
        var path = inputData.FilePath;
        var pathType = inputData.FilePathType;

        var yamlData = _fileLoader.Load(path, pathType);
        var serializedSchema = _schemaLoader.Load();

        foreach (var yamlFileData in yamlData)
        {
            Console.WriteLine($"Validating '{yamlFileData.Key}'");
            var result = _validator.Validate(serializedSchema, yamlFileData.Value);
            Console.WriteLine($"Validation {(result.SchemaValid ? "Passed" : "Failed")}");

            foreach (var error in result.TraversalResults)
            {
                Console.WriteLine($"{error}");
            }
            Console.WriteLine();
        }
    }
}
