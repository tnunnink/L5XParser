﻿using System.Collections.Generic;
using System.Linq;
using L5Sharp.Components;
using L5Sharp.Core;
using L5Sharp.Entities;

namespace L5Sharp.Extensions;

/// <summary>
/// Container for all public extensions methods that add functionality to the base components of the library.
/// </summary>
public static class LogixExtensions
{
    /// <summary>
    /// Returns all <see cref="DataType"/> instances that are dependent on the specified data type name.
    /// </summary>
    /// <param name="dataTypes">The logix collection of data types.</param>
    /// <param name="name">The name of the data type for which to find dependencies.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="DataType"/> that are dependent on the specified data type name.</returns>
    /// <remarks>
    /// This is mostly here as an example of how one could extend the API of certain component collections to
    /// add custom XML queries against the source L5X and return materialized components.
    /// </remarks>
    public static IEnumerable<DataType> DependentsOf(this ILogixCollection<DataType> dataTypes, string name)
    {
        return dataTypes.Container.Descendants(L5XName.DataType)
            .Where(e => e.Descendants(L5XName.Member).Any(m => m.Attribute(L5XName.DataType)?.Value == name))
            .Select(e => new DataType(e));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static LogixTextQuery Text(this LogixContent content) => new(content);

    /// <summary>
    /// Creates a tag name lookup for the current collection of <c>Rung</c> logic.
    /// </summary>
    /// <param name="rungs">A collection of <see cref="Rung"/> logic.</param>
    /// <returns>A <see cref="Dictionary{TKey,TValue}"/> where each tag name withing the rungs is a key and it's
    /// corresponding value is a <see cref="List{T}"/> containing all the <see cref="Rung"/> referencing
    /// found in the collection.</returns>
    /// <remarks>
    /// This is useful for performing quick lookup of logic references by tag name.
    /// </remarks>
    public static Dictionary<TagName, List<Rung>> ToTagLookup(this IEnumerable<Rung> rungs)
    {
        var results = new Dictionary<TagName, List<Rung>>();

        foreach (var rung in rungs)
        {
            var tags = rung.Text.Tags();

            foreach (var tag in tags)
            {
                if (!results.ContainsKey(tag))
                {
                    results.Add(tag, new List<Rung> { rung });
                    continue;
                }

                results[tag].Add(rung);
            }
        }

        return results;
    }

    /// <summary>
    /// Returns all referenced tag names and their corresponding list of <see cref="NeutralText"/> logic references in
    /// the current collection of <see cref="NeutralText"/>.
    /// </summary>
    /// <param name="logic">A collection of <see cref="NeutralText"/> rung logic.</param>
    /// <returns>A <see cref="Dictionary{TKey,TValue}"/> where each tag name is a key and it's corresponding value is
    /// a <see cref="List{T}"/> containing all the logic referencing the tag found in the file.</returns>
    /// <remarks>
    /// This is useful for performing quick lookup of logic references by tag name.
    /// </remarks>
    public static Dictionary<TagName, List<NeutralText>> ToTagLookup(this IEnumerable<NeutralText> logic)
    {
        var results = new Dictionary<TagName, List<NeutralText>>();

        foreach (var text in logic)
        {
            var tags = text.Tags();

            foreach (var tag in tags)
            {
                if (!results.ContainsKey(tag))
                {
                    results.Add(tag, new List<NeutralText> { text });
                    continue;
                }

                results[tag].Add(text);
            }
        }

        return results;
    }

    /// <summary>
    /// Gets a lookup of all <see cref="TagMember"/> within the current <see cref="LogixContent"/> file.
    /// </summary>
    /// <param name="content">The current <see cref="LogixContent"/> instance.</param>
    /// <returns>A <see cref="ILookup{TKey,TValue}"/> of all tag names and their corresponding
    /// <see cref="TagMember"/> instance in the L5X file.</returns>
    /// <remarks>This is helper to get a tag lookup for fast access to finding tags within the L5X file. Note that some
    /// tags may have multiple <see cref="TagMember"/> instance if they are scoped (program) tags with the same tag name.</remarks>
    public static ILookup<TagName, TagMember> TagLookup(this LogixContent content) =>
        content.Find<Tag>().SelectMany(t => t.MembersAndSelf()).ToLookup(t => t.TagName, t => t);
}