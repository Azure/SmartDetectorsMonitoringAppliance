// Create string-to-<T> dictionary object type
type IDict<T> = { [id: string]: T };
type IDictionary = IDict<any>;