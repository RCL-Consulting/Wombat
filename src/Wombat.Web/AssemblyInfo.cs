using System.Runtime.CompilerServices;

// T133. BuilderSchemaModel is the only schema-authoring path in the product and had no test at all,
// which is how T119's and T126's root pointers came to be erased on every operator draft save
// without anything going red. Mirrors the two lines Wombat.Infrastructure already carries.
[assembly: InternalsVisibleTo("Wombat.Web.Tests")]
