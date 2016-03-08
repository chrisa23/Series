# Series

Simple circular buffer for time series data.  Indexing is from most recent while enumeration is from beginning.

	let s = new Series<float>(20) 
	s.Add 2.0
	s.Add 3.0
	s.Add 4.0
	let v = s.[0]//4.0
