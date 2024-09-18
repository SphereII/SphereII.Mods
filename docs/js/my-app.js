// -- 1. Setup app
var myApp = new Framework7({
    // Disable App's automatic initialization
    init: false,
    // If it is webapp, we can enable hash navigation
    pushState: true,
    // Enable Template7 rendering for pages
    template7Pages: true
});

// -- 2. Export selectors engine
var $$ = Dom7;

// -- 3. Create views

// Left view
var leftView = myApp.addView('.view-left', {
    // Because we use fixed-through navbar we can enable dynamic navbar
    dynamicNavbar: true
});

// Main view
var mainView = myApp.addView('.view-main', {
    // Because we use fixed-through navbar we can enable dynamic navbar
    dynamicNavbar: true
});

// /!\ Trick to load the default page: use the  
mainView.router.reloadPage(mainView.container.dataset.hndDefaultTopic);

// -- 4. Setup events

myApp.onPageInit('index-left', function(page) {
    // Left view -> toc
    leftView.router.load({
        url: "_toc.html",
        animatePages: false
    });
});

myApp.onPageInit('search', function(page) {
    // Display message in search list
    var displaySearchMessage = function(searchList, msg) {
        searchList.html('<li class="item-content"><div class="item-inner"><div class="item-title">' + msg + '</div></div></li>');
    };
    // Custom search setup
    var searchBar = myApp.searchbar('.searchbar', {
        customSearch: true,
        searchList: '#search-result',
        onSearch: function(s) {
            // Make sure search data has been loaded
            if (!window.bSearchDataLoaded) {
                displaySearchMessage('Search data not loaded yet');
                return;
            }
            // Perform search
            var searchEngine = new HndJsSe;
            searchEngine.ParseInput(s.query);
            var searchResults = searchEngine.PerformSearch(oWl);
            // Display results        
            if (!searchResults || !searchResults.length) {
                displaySearchMessage(s.searchList, 'No results or invalid input');
            }
            else {
                s.searchList.html('');
				for (var nCnt = 0; nCnt < searchResults.length; nCnt++) {
					if (searchResults[nCnt][0] < aTl.length) {
						s.searchList.append('<li><a class="item-link item-content" href="' + aTl[searchResults[nCnt][0]][0] + '" data-view=".view-main"><div class="item-inner"><div class="item-title">' + unescape(aTl[searchResults[nCnt][0]][1]) + '</div></div></a></li>');
					}
				}
            }
        },
        onClear: function(s) {
            displaySearchMessage(s.searchList, 'No results or invalid input');
        }
    });
});

// -- 5. Init App

// /!\ Init app manually as we set "myApp.init = false"
myApp.init();

// -- 6. Load search data
(function() {
    var se = document.createElement('script'); se.type = 'text/javascript'; se.async = true;
    se.src = 'js/hndsd.js';
    var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(se, s);
})();