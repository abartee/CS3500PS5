// Alex Bartee PS4b

using System;
using System.Collections.Generic;

namespace Dependencies
{
    /// <summary>
    /// A DependencyGraph can be modeled as a set of dependencies, where a dependency is an ordered 
    /// pair of strings.  Two dependencies (s1,t1) and (s2,t2) are considered equal if and only if 
    /// s1 equals s2 and t1 equals t2.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that the dependency (s,t) is in DG 
    ///    is called the dependents of s, which we will denote as dependents(s).
    ///        
    ///    (2) If t is a string, the set of all strings s such that the dependency (s,t) is in DG 
    ///    is called the dependees of t, which we will denote as dependees(t).
    ///    
    /// The notations dependents(s) and dependees(s) are used in the specification of the methods of this class.
    ///
    /// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    ///     dependents("a") = {"b", "c"}
    ///     dependents("b") = {"d"}
    ///     dependents("c") = {}
    ///     dependents("d") = {"d"}
    ///     dependees("a") = {}
    ///     dependees("b") = {"a"}
    ///     dependees("c") = {"a"}
    ///     dependees("d") = {"b", "d"}
    ///     
    /// All of the methods below require their string parameters to be non-null.  This means that 
    /// the behavior of the method is undefined when a string parameter is null.  
    ///
    /// IMPORTANT IMPLEMENTATION NOTE
    /// 
    /// The simplest way to describe a DependencyGraph and its methods is as a set of dependencies, 
    /// as discussed above.
    /// 
    /// However, physically representing a DependencyGraph as, say, a set of ordered pairs will not
    /// yield an acceptably efficient representation.  DO NOT USE SUCH A REPRESENTATION.
    /// 
    /// You'll need to be more clever than that.  Design a representation that is both easy to work
    /// with as well acceptably efficient according to the guidelines in the PS3 writeup. Some of
    /// the test cases with which you will be graded will create massive DependencyGraphs.  If you
    /// build an inefficient DependencyGraph this week, you will be regretting it for the next month.
    /// </summary>
    public class DependencyGraph
    {
        /// <summary>
        /// Dictionary that tracks all values that the key is dependent on
        /// </summary>
        private Dictionary<string, List<string>> Dependents;
        /// <summary>
        /// Dictionary that tracks all values that the key is a dependent of
        /// </summary>
        private Dictionary<string, List<string>> Dependees;
        /// <summary>
        /// Tracks the size, incrementing when adding, decrementing when removing
        /// </summary>
        private int size;
        /// <summary>
        /// Creates a DependencyGraph containing no dependencies.
        /// </summary>
        public DependencyGraph()
        {
            Dependents = new Dictionary<string, List<string>>();
            Dependees = new Dictionary<string, List<string>>();
            size = 0;
        }
        /// <summary>
        /// New constructor for PS4 to copy a dependency graph
        /// </summary>
        /// <param name="original"></param>
        public DependencyGraph( DependencyGraph original)
        {
            if (original == null)
            {
                throw new NullReferenceException("Original cannot be null");
            }
            Dependents = new Dictionary<string, List<string>>();
            Dependees = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, List<string>> item in original.Dependents)
            {
                List<string> dependents = new List<string>();
                foreach (string dep in item.Value)
                {
                    dependents.Add(dep);
                }
                Dependents.Add(item.Key, dependents);
            }
            foreach (KeyValuePair<string, List<string>> item in original.Dependees)
            {
                List<string> dependees = new List<string>();
                foreach (string dep in item.Value)
                {
                    dependees.Add(dep);
                }
                Dependees.Add(item.Key, dependees);
            }
            size = original.size;
        }

        /// <summary>
        /// The number of dependencies in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependents(string s)
        {

            if (s == null)
            {
                throw new ArgumentNullException("s Cannot be null");
            }
            return Dependents.ContainsKey(s);
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.  Requires s != null.
        /// </summary>
        public bool HasDependees(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s Cannot be null");
            }
            return Dependees.ContainsKey(s);
        }

        /// <summary>
        /// Enumerates dependents(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s Cannot be null");
            }
            if (Dependents.ContainsKey(s)){
                return Dependents[s];
            }
            return new List<string>();
        }

        /// <summary>
        /// Enumerates dependees(s).  Requires s != null.
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s Cannot be null");
            }
            if (Dependees.ContainsKey(s))
            {
                return Dependees[s];
            }
            return new List<string>();
        }

        /// <summary>
        /// Adds the dependency (s,t) to this DependencyGraph.
        /// This has no effect if (s,t) already belongs to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void AddDependency(string s, string t)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s Cannot be null");
            }
            if (t == null)
            {
                throw new ArgumentNullException("t Cannot be null");
            }
            if (Dependents.ContainsKey(s))
            {
                //Check for duplicate and kick out of method if so
                if (Dependents[s].Contains(t))
                {
                    return;
                }
                Dependents[s].Add(t);
                size++;
            }
            else
            {
                Dependents.Add(s, new List<string>() { t });
                size++;
            }
            if (Dependees.ContainsKey(t))
            {
                Dependees[s].Add(s);
            }
            else
            {
                Dependees.Add(t, new List<string>() { s });
            }
        }

        /// <summary>
        /// Removes the dependency (s,t) from this DependencyGraph.
        /// Does nothing if (s,t) doesn't belong to this DependencyGraph.
        /// Requires s != null and t != null.
        /// </summary>
        public void RemoveDependency(string s, string t)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s Cannot be null");
            }
            if (t == null)
            {
                throw new ArgumentNullException("t Cannot be null");
            }
            if (Dependents.ContainsKey(s))
            {
                if (Dependents[s].Remove(t))
                {
                    Dependees.ContainsKey(t);
                    Dependees[t].Remove(s);
                    size--;
                    // Remove empty dependents and dependees
                    if (Dependents[s].Count == 0)
                    {
                        Dependents.Remove(s);
                    }
                    if (Dependees[t].Count == 0)
                    {
                        Dependees.Remove(t);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (s,r).  Then, for each
        /// t in newDependents, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s Cannot be null");
            }
            if (newDependents == null)
            {
                throw new ArgumentNullException("newDependents Cannot be null");
            }
            if (Dependents.ContainsKey(s))
            {
                while (Dependents[s].Count != 0)
                {
                    RemoveDependency(s, Dependents[s][0]);
                }
                foreach (string t in newDependents)
                {
                    if (t == null)
                    {
                        throw new ArgumentNullException("newDependents Cannot contain null");
                    }
                    AddDependency(s, t);
                }
            }
        }

        /// <summary>
        /// Removes all existing dependencies of the form (r,t).  Then, for each 
        /// s in newDependees, adds the dependency (s,t).
        /// Requires s != null and t != null.
        /// </summary>
        public void ReplaceDependees(string t, IEnumerable<string> newDependees)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t Cannot be null");
            }
            if (newDependees == null)
            {
                throw new ArgumentNullException("newDependees Cannot be null");
            }
            if (Dependees.ContainsKey(t))
            {
                while (Dependees[t].Count != 0)
                {
                    RemoveDependency(Dependees[t][0], t);
                }
                foreach (string s in newDependees)
                {
                    if (s == null)
                    {
                        throw new ArgumentNullException("newDependees Cannot contain null");
                    }
                    AddDependency(s, t);
                }
            }
        }
    }
}
