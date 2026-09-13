$(function() {
 
            var tabs = $('.tabs');
 
            setTimeout(function() {
                        var activeItem = tabs.find('a.active');
                        var activeWidth = activeItem.innerWidth();  
 
                        tabs.find('.selector').css({
                          'left': activeItem.position().left + 'px',
                          'width': activeWidth + 'px'
                        });
            }, 50);
 
            tabs.find('a').bind('click', function(e){
              var self = $(this);
             
              tabs.find('a').removeClass('active');
              $(this).addClass('active');
             
              var width = $(this).innerWidth();
              var pos = $(this).position();
              $('.selector').css({
                        'left':pos.left + 'px',
                        'width': width + 'px'
              });
 
                        window.location.href = self.data('url');
            });
 
           
});

