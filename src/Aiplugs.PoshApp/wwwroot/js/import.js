const links = document.querySelectorAll('link[rel="import"]')

Array.prototype.forEach.call(links, function (link) {
  let template = link.import.querySelector('template')
  document.querySelector('body').appendChild(template)
})