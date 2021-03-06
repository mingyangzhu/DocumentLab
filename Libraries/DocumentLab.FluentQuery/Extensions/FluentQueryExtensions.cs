﻿namespace DocumentLab
{
  using Newtonsoft.Json;
  using Newtonsoft.Json.Linq;
  using PageInterpreter;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public static class FluentQueryExtensions
  {
    internal static FluentQuery AppendToScript(this FluentQuery query, string operation)
    {
      query.Script += " " + operation;

      return query;
    }

    /// <summary>
    /// Initiates a table query, a series of TableColumn method calls needs to follow this one. 
    /// </summary>
    /// <returns>A DocumentLab FluentQuery with a script extension that initates the table query</returns>
    public static FluentQuery Table(this FluentQuery response) => response.AppendToScript("Table");

    /// <summary>
    /// Specifies a tablecolumn for the preceding table query initializer
    /// </summary>
    /// <param name="columnName">The name to assign for the column in the result output</param>
    /// <param name="columnTextType">The text type considered valid in the table row's data for this column</param>
    /// <param name="labels">Labels that can be evaluated to identify the table on the page</param>
    /// <returns>A DocumentLab FluentQuery with a script extension that contains the table column definition</returns>
    public static FluentQuery TableColumn(this FluentQuery response, string columnName, string columnTextType, params TextType[] labels)
      => response.TableColumn(columnName, columnTextType, labels.Select(x => x.ToString()).ToArray());

    /// <summary>
    /// Specifies a tablecolumn for the preceding table query initializer
    /// </summary>
    /// <param name="columnName">The name to assign for the column in the result output</param>
    /// <param name="columnTextType">The text type considered valid in the table row's data for this column</param>
    /// <param name="labels">Labels that can be evaluated to identify the table on the page</param>
    /// <returns>A DocumentLab FluentQuery with a script extension that contains the table column definition</returns>
    public static FluentQuery TableColumn(this FluentQuery response, string columnName, string columnTextType, params string[] labels)
      => response.AppendToScript($"'{columnName}': [{columnTextType}({string.Join("||", labels)})]");

    /// <summary>
    /// Following a pattern predicate or a capture, specify to move *Up* from there to look for the next element in the document for the next operation.
    /// </summary>
    /// <returns>A DocumentLab FluentQuery with a script extension that performs the traversal.</returns>
    public static FluentQuery Up(this FluentQuery response) => response.AppendToScript("Up");

    /// <summary>
    /// Following a pattern predicate or a capture, specify to move *Down* from there to look for the next element in the document for the next operation.
    /// </summary>
    /// <returns>A DocumentLab FluentQuery with a script extension that performs the traversal.</returns>
    public static FluentQuery Down(this FluentQuery response) => response.AppendToScript("Down");

    /// <summary>
    /// Following a pattern predicate or a capture, specify to move *Left* from there to look for the next element in the document for the next operation.
    /// </summary>
    /// <returns>A DocumentLab FluentQuery with a script extension that performs the traversal.</returns>
    public static FluentQuery Left(this FluentQuery response) => response.AppendToScript("Left");

    /// <summary>
    /// Following a pattern predicate or a capture, specify to move *Right* from there to look for the next element in the document for the next operation.
    /// </summary>
    /// <returns>A DocumentLab FluentQuery with a script extension that performs the traversal.</returns>
    public static FluentQuery Right(this FluentQuery response) => response.AppendToScript("Right");

    /// <summary>
    /// Adds a text type and optional text match predicate to the pattern we want to match in a document.
    /// </summary>
    /// <param name="textType">The text type we want to match in the pattern</param>
    /// <param name="matchText">*Optional* Adds that the text type match also needs to match the text specified. This works by checking if the string we're evaluating with from the document contains the text we specify here + a Levensthein distance 2 (by default) check. Therefore the text here can be an abbreviation of common terms or at least in some simplified form in order to make it more durable to differences that might occur in OCR results.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script extension that performs the match.</returns>
    public static FluentQuery Match(this FluentQuery response, TextType textType, params string[] matchText) => MatchBase(response, new string[] { textType.ToString() }, matchText);

    /// <summary>
    /// Adds a text type and optional text match predicate to the pattern we want to match in a document.
    /// </summary>
    /// <param name="textType">The text type we want to match in the pattern</param>
    /// <param name="matchText">*Optional* Adds that the text type match also needs to match the text specified. This works by checking if the string we're evaluating with from the document contains the text we specify here + a Levensthein distance 2 (by default) check. Therefore the text here can be an abbreviation of common terms or at least in some simplified form in order to make it more durable to differences that might occur in OCR results.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script extension that performs the match.</returns>
    public static FluentQuery Match(this FluentQuery response, string textType, params string[] matchText) => MatchBase(response, new string[] { textType }, matchText);

    /// <summary>
    /// Adds a text type and optional text match predicate to the pattern we want to match in a document.
    /// </summary>
    /// <param name="textType">The text type we want to match in the pattern</param>
    /// <param name="matchText">*Optional* Adds that the text type match also needs to match the text specified. This works by checking if the string we're evaluating with from the document contains the text we specify here + a Levensthein distance 2 (by default) check. Therefore the text here can be an abbreviation of common terms or at least in some simplified form in order to make it more durable to differences that might occur in OCR results.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script extension that performs the match.</returns>
    public static FluentQuery Match(this FluentQuery response, TextType[] textType, params string[] matchText) => MatchBase(response, textType.Select(x => x.ToString()).ToArray(), matchText);

    /// <summary>
    /// Adds a text type and optional text match predicate to the pattern we want to match in a document.
    /// </summary>
    /// <param name="textType">The text type we want to match in the pattern</param>
    /// <param name="matchText">*Optional* Adds that the text type match also needs to match the text specified. This works by checking if the string we're evaluating with from the document contains the text we specify here + a Levensthein distance 2 (by default) check. Therefore the text here can be an abbreviation of common terms or at least in some simplified form in order to make it more durable to differences that might occur in OCR results.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script extension that performs the match.</returns>
    public static FluentQuery Match(this FluentQuery response, string[] textType, params string[] matchText) => MatchBase(response, textType, matchText);


    /// <summary>
    /// At the end of a query, calling this method performs a capture operation on the document using the specified text type as a match predicate. The value returned is the value that corresponds to the pattern on the document.
    /// </summary>
    /// <param name="captureTextType">The text type predicate the capture must match for a result to be considered valid</param>
    /// <returns>Value extracted from document.</returns>
    public static string Capture(this FluentQuery response, TextType captureTextType) => CaptureBase(response, new string[] { captureTextType.ToString() });

    /// <summary>
    /// At the end of a query, calling this method performs a capture operation on the document using the specified text type as a match predicate. The value returned is the value that corresponds to the pattern on the document.
    /// </summary>
    /// <param name="captureTextType">The text type predicate the capture must match for a result to be considered valid</param>
    /// <returns>Value extracted from document.</returns>
    public static string Capture(this FluentQuery response, string captureTextType) => CaptureBase(response, new string[] { captureTextType });

    /// <summary>
    /// At the end of a query, calling this method performs a capture operation on the document using the specified text type as a match predicate. The value returned is the value that corresponds to the pattern on the document.
    /// </summary>
    /// <param name="captureTextType">The text type predicate the capture must match for a result to be considered valid</param>
    /// <returns>Value extracted from document.</returns>
    public static string Capture(this FluentQuery response, TextType[] captureTextType) => CaptureBase(response, captureTextType.Select(x => x.ToString()).ToArray());

    /// <summary>
    /// At the end of a query, calling this method performs a capture operation on the document using the specified text type as a match predicate. The value returned is the value that corresponds to the pattern on the document.
    /// </summary>
    /// <param name="captureTextType">The text type predicate the capture must match for a result to be considered valid</param>
    /// <returns>Value extracted from document.</returns>
    public static string Capture(this FluentQuery response, string[] captureTextType) => CaptureBase(response, captureTextType);


    /// <summary>
    /// Executes a multiple capture query
    /// </summary>
    /// <param name="fluentQuery">A FluentQuery object containing the script built so far.</param>
    /// <returns>The data specified for capture in the query in a dictionary.</returns>
    public static Dictionary<string, string> Capture(this FluentQuery fluentQuery, Func<FluentQuery, FluentQuery> query)
    {
      fluentQuery = query(fluentQuery);

      if (fluentQuery.QueryType != QueryType.MultiCapture)
      {
        throw new FluentQueryException("A multi capture query needs to have multiple captures specified");
      }

      return fluentQuery.ExecuteQuery();
    }

    /// <summary>
    /// Captures the value of the text matched in the document and includes it in the extracted output.
    /// </summary>
    /// <param name="captureTextType">The text type we want to capture in the document. In a pattern the text type specified in a pattern must yield a positive match in order for the capture to be valid.</param>
    /// <param name="propertyName">*Optional* Specify a name for the property associated with the capture. This is only applicable for multi-capture patterns.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script extension that performs the capture.</returns>
    public static FluentQuery Capture(this FluentQuery response, TextType captureTextType, string propertyName = "")
    {
      return CaptureNamedBase(response, new string[] { captureTextType.ToString() }, propertyName);
    }

    /// <summary>
    /// Captures the value of the text matched in the document and includes it in the extracted output.
    /// </summary>
    /// <param name="captureTextType">The text type we want to capture in the document. In a pattern the text type specified in a pattern must yield a positive match in order for the capture to be valid.</param>
    /// <param name="propertyName">*Optional* Specify a name for the property associated with the capture. This is only applicable for multi-capture patterns.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script extension that performs the capture.</returns>
    public static FluentQuery Capture(this FluentQuery response, string captureTextType, string propertyName = "") => CaptureNamedBase(response, new string[] { captureTextType }, propertyName);

    /// <summary>
    /// Captures the value of the text matched in the document and includes it in the extracted output.
    /// </summary>
    /// <param name="captureTextType">The text type we want to capture in the document. In a pattern the text type specified in a pattern must yield a positive match in order for the capture to be valid.</param>
    /// <param name="propertyName">*Optional* Specify a name for the property associated with the capture. This is only applicable for multi-capture patterns.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script extension that performs the capture.</returns>
    public static FluentQuery Capture(this FluentQuery response, TextType[] captureTextType, string propertyName = "")
    {
      return CaptureNamedBase(response, captureTextType.Select(x => x.ToString()).ToArray(), propertyName);
    }

    /// <summary>
    /// Captures the value of the text matched in the document and includes it in the extracted output.
    /// </summary>
    /// <param name="captureTextType">The text type we want to capture in the document. In a pattern the text type specified in a pattern must yield a positive match in order for the capture to be valid.</param>
    /// <param name="propertyName">*Optional* Specify a name for the property associated with the capture. This is only applicable for multi-capture patterns.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script extension that performs the capture.</returns>
    public static FluentQuery Capture(this FluentQuery response, string[] captureTextType, string propertyName = "") => CaptureNamedBase(response, captureTextType, propertyName);


    /// <summary>
    /// Performs a Right-Down search from the previous predcate. A predicate or capture should follow. This is implicitly already used in the FindValueForLabel and GetValueForLabel methods. Use this method if you want to create more detailed patterns using RD.
    /// </summary>
    /// <param name="maxSteps">The maximum distance in cells between the previous predicate and the next predicate for the pattern to be valid.</param>
    /// <returns></returns>
    public static FluentQuery RightDownSearch(this FluentQuery response, int maxSteps) => response.AppendToScript($"RD {maxSteps}");

    /// <summary>
    /// Finds a value by a label's text using the Right-Down search algorithm. The value closest to the label's text in the right or down direction in the document will be chosen as the result.
    /// </summary>
    /// <param name="labelInDocument">The label we expect to find in the document, valid matches can be separated by ||</param>
    /// <param name="textTypeOfValue">Specifies the text type the capture operation needs to match</param>
    /// <param name="maxSteps">The maximum distance in terms of DocumentLab grid cells the label-value in the document can be. This is by default 6 which is sufficient for close-by elements but can be made longer or shorter depending on the type of the document.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script that performs the value extraction.</returns>
    public static string FindValueForLabel(this FluentQuery response, TextType textTypeOfValue = TextType.Text, int maxSteps = 6, params string[] labelInDocument)
      => FindValueForLabelBase(response, new string[] { textTypeOfValue.ToString() }, maxSteps, labelInDocument);

    /// <summary>
    /// Finds a value by a label's text using the Right-Down search algorithm. The value closest to the label's text in the right or down direction in the document will be chosen as the result.
    /// </summary>
    /// <param name="textTypeOfValue">Specifies the text type the capture operation needs to match</param>
    /// <param name="maxSteps">The maximum distance in terms of DocumentLab grid cells the label-value in the document can be. This is by default 6 which is sufficient for close-by elements but can be made longer or shorter depending on the type of the document.</param>
    /// <param name="labelInDocument">The label we expect to find in the document</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script that performs the value extraction.</returns>
    public static string FindValueForLabel(this FluentQuery response, string textTypeOfValue = "Text", int maxSteps = 6, params string[] labelInDocument)
      => FindValueForLabelBase(response, new string[] { textTypeOfValue }, maxSteps, labelInDocument);

    /// <summary>
    /// Finds a value by a label's text using the Right-Down search algorithm. The value closest to the label's text in the right or down direction in the document will be chosen as the result.
    /// </summary>
    /// <param name="labelInDocument">The label we expect to find in the document, valid matches can be separated by ||</param>
    /// <param name="textTypeOfValue">Specifies the text type the capture operation needs to match</param>
    /// <param name="maxSteps">The maximum distance in terms of DocumentLab grid cells the label-value in the document can be. This is by default 6 which is sufficient for close-by elements but can be made longer or shorter depending on the type of the document.</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script that performs the value extraction.</returns>
    public static string FindValueForLabel(this FluentQuery response, TextType[] textTypeOfValue, int maxSteps = 6, params string[] labelInDocument)
      => FindValueForLabelBase(response, textTypeOfValue.Select(x => x.ToString()).ToArray(), maxSteps, labelInDocument);

    /// <summary>
    /// Finds a value by a label's text using the Right-Down search algorithm. The value closest to the label's text in the right or down direction in the document will be chosen as the result.
    /// </summary>
    /// <param name="textTypeOfValue">Specifies the text type the capture operation needs to match</param>
    /// <param name="maxSteps">The maximum distance in terms of DocumentLab grid cells the label-value in the document can be. This is by default 6 which is sufficient for close-by elements but can be made longer or shorter depending on the type of the document.</param>
    /// <param name="labelInDocument">The label we expect to find in the document</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script that performs the value extraction.</returns>
    public static string FindValueForLabel(this FluentQuery response, string[] textTypeOfValue, int maxSteps = 6, params string[] labelInDocument)
      => FindValueForLabelBase(response, textTypeOfValue, maxSteps, labelInDocument);


    /// <summary>
    /// Gets value by label's text given that we know in which direction relative to the label the value is.
    /// </summary>
    /// <param name="labelInDocument">The label we expect to find in the document, valid matches can be separated by ||</param>
    /// <param name="textTypeOfValue">Specifies the text type the capture operation needs to match</param>
    /// <param name="direction">Direction relative to the label the value should be located at in the document</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script that performs the value extraction.</returns>
    public static string GetValueAtLabel(this FluentQuery response, Direction direction, TextType textTypeOfValue = TextType.Text, params string[] labelInDocument)
      => GetValueAtLabelBase(response, direction, new string[] { textTypeOfValue.ToString() }, labelInDocument);

    /// <summary>
    /// Gets value by label's text given that we know in which direction relative to the label the value is.
    /// </summary>
    /// <param name="labelInDocument">The label we expect to find in the document, valid matches can be separated by ||</param>
    /// <param name="direction">Direction relative to the label the value should be located at in the document</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script that performs the value extraction.</returns>
    public static string GetValueAtLabel(this FluentQuery response, Direction direction, string textTypeOfValue = "Text", params string[] labelInDocument)
      => GetValueAtLabelBase(response, direction, new string[] { textTypeOfValue }, labelInDocument);

    /// <summary>
    /// Gets value by label's text given that we know in which direction relative to the label the value is.
    /// </summary>
    /// <param name="labelInDocument">The label we expect to find in the document, valid matches can be separated by ||</param>
    /// <param name="textTypeOfValue">Specifies the text type the capture operation needs to match</param>
    /// <param name="direction">Direction relative to the label the value should be located at in the document</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script that performs the value extraction.</returns>
    public static string GetValueAtLabel(this FluentQuery response, Direction direction, TextType[] textTypeOfValue, params string[] labelInDocument)
      => GetValueAtLabelBase(response, direction, textTypeOfValue.Select(x => x.ToString()).ToArray(), labelInDocument);

    /// <summary>
    /// Gets value by label's text given that we know in which direction relative to the label the value is.
    /// </summary>
    /// <param name="labelInDocument">The label we expect to find in the document, valid matches can be separated by ||</param>
    /// <param name="direction">Direction relative to the label the value should be located at in the document</param>
    /// <returns>Returns a DocumentLab FluentQuery with a script that performs the value extraction.</returns>
    public static string GetValueAtLabel(this FluentQuery response, Direction direction, string[] textTypeOfValue, params string[] labelInDocument)
      => GetValueAtLabelBase(response, direction, textTypeOfValue, labelInDocument);

    /// <summary>
    /// Gets all values of the specified text type in a document.
    /// </summary>
    /// <param name="textType">The text type to capture all instances of in a document.</param>
    /// <returns>A DocumentLab FluentQuery with a script extension that performs the Any operation.</returns>
    public static string[] GetAny(this FluentQuery response, TextType textType)
      => GetAnyBase(response, new string[] { textType.ToString() });

    /// <summary>
    /// Gets all values of the specified text type in a document.
    /// </summary>
    /// <param name="textType">The text type to capture all instances of in a document.</param>
    /// <returns>A DocumentLab FluentQuery with a script extension that performs the Any operation.</returns>
    public static string[] GetAny(this FluentQuery response, string textType)
      => GetAnyBase(response, new string[] { textType });

    /// <summary>
    /// Gets all values of the specified text type in a document.
    /// </summary>
    /// <param name="textType">The text type to capture all instances of in a document.</param>
    /// <returns>A DocumentLab FluentQuery with a script extension that performs the Any operation.</returns>
    public static string[] GetAny(this FluentQuery response, TextType[] textType)
      => GetAnyBase(response, textType.Select(x => x.ToString()).ToArray());

    /// <summary>
    /// Gets all values of the specified text type in a document.
    /// </summary>
    /// <param name="textType">The text type to capture all instances of in a document.</param>
    /// <returns>A DocumentLab FluentQuery with a script extension that performs the Any operation.</returns>
    public static string[] GetAny(this FluentQuery response, string[] textType)
      => GetAnyBase(response, textType);

    /// <summary>
    /// Specifies which subset of the page to limit the query to. 
    /// </summary>
    /// <param name="subset">A subset definition. Use the static methods in the Subset class to instantiate.</param>
    /// <returns>A Documentab FluentQuery with a script extension that performs the subset operation</returns>
    public static FluentQuery Subset(this FluentQuery response, Subset subset)
      => SubsetBase(response, new Subset[] { subset });

    /// <summary>
    /// Specifies which subsets of the page to limit the query to. 
    /// </summary>
    /// <param name="subsets">A subset definition array. Use the static methods in the Subset class to instantiate.</param>
    /// <returns>A Documentab FluentQuery with a script extension that performs the subset operation</returns>
    public static FluentQuery Subset(this FluentQuery response, Subset[] subsets)
      => SubsetBase(response, subsets);

    private static string ExecuteSingleCapture(this FluentQuery fluentQuery)
    {
      return fluentQuery.ExecuteQuery().FirstOrDefault().Value;
    }

    private static FluentQuery SubsetBase(FluentQuery response, Subset[] subset)
    {
      return response.AppendToScript($"Subset({string.Join(", ", subset.ToString())})");
    }

    private static FluentQuery MatchBase(FluentQuery response, string[] textType, string[] matchText)
    {
      return response.AppendToScript(matchText.Count() > 0 ? $"{Or(textType.Select(x => $"{x}({string.Join("||", matchText)})").ToArray())}" : Or(textType));
    }

    private static string CaptureBase(FluentQuery response, string[] captureTextType)
    {
      if (response.QueryType == QueryType.None)
      {
        response.QueryType = QueryType.SingleCapture;
      }

      return ExecuteSingleCapture(response.AppendToScript($"[{Or(captureTextType)}]"));
    }

    private static FluentQuery CaptureNamedBase(FluentQuery response, string[] captureTextType, string propertyName)
    {
      response.QueryType = QueryType.MultiCapture;

      if (string.IsNullOrWhiteSpace(propertyName))
      {
        throw new FluentQueryException("The specified pattern has multiple captures, a property name must be specified when capturing more than one value.");
      }

      return response.AppendToScript((!string.IsNullOrWhiteSpace(propertyName) ? $"'{propertyName}': " : string.Empty) + $"[{Or(captureTextType)}]");
    }

    private static string FindValueForLabelBase(FluentQuery response, string[] textTypeOfValue, int maxSteps, string[] labelInDocument)
    {
      response.QueryType = QueryType.SingleCapture;
      response.AppendToScript($"Text({string.Join("||", labelInDocument)}) RD {maxSteps} [{Or(textTypeOfValue)}]");

      return ExecuteSingleCapture(response);
    }

    private static string GetValueAtLabelBase(FluentQuery response, Direction direction, string[] textTypeOfValue, string[] labelInDocument)
    {
      response.QueryType = QueryType.SingleCapture;
      response.AppendToScript($"Text({string.Join("||", labelInDocument)}) {direction} [{Or(textTypeOfValue)}]");

      return ExecuteSingleCapture(response);
    }

    private static string[] GetAnyBase(FluentQuery response, string[] textType)
    {
      response.QueryType = QueryType.Any;
      response.AppendToScript($"Any [{Or(textType)}]");

      return response.ExecuteQuery().Select(x => x.Value).ToArray();
    }

    private static string Or(string[] vs) => string.Join("||", vs);

    private static Dictionary<string, string> ExecuteQuery(this FluentQuery fluentQuery)
    {
      if (fluentQuery.QueryType == QueryType.None)
      {
        throw new FluentQueryException("Query includes no capture tokens. Nothing to query for.");
      }

      fluentQuery.AppendToScript(";");

      var interpretationResultJson = fluentQuery
        .Interpreter
        .Interpret(fluentQuery.AnalyzedPage, fluentQuery.Script)
        .ConvertToJson(fluentQuery.Script);

      if (string.IsNullOrWhiteSpace(interpretationResultJson))
      {
        return null;
      }

      var json = JObject.Parse(interpretationResultJson)[FluentQueryConstants.GeneratedScriptQuery];
      Dictionary<string, string> deserializedResult = null;
      switch (fluentQuery.QueryType)
      {
        case QueryType.SingleCapture:
          deserializedResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(interpretationResultJson);
          break;
        case QueryType.MultiCapture:
          deserializedResult = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.ToString());
          break;
        case QueryType.Any:
          deserializedResult = JsonConvert.DeserializeObject<string[]>(json.ToString())
            .Select((x, i) => new KeyValuePair<int, string>(i, x))
            .ToDictionary(x => x.Key.ToString(), x => x.Value);
          break;
        default: throw new FluentQueryException("Query type is invalid");
      }

      if (deserializedResult == null || deserializedResult.Count == 0)
      {
        return null;
      }

      return deserializedResult;
    }
  }
}
